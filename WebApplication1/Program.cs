using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WebApplication1;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using WebApplication1.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Начало настройки приложения
Log.Information("Starting application configuration");

// Настройки JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "default_key_that_should_be_replaced");

Log.Information("JWT Configuration:");
Log.Information("  Issuer: {Issuer}", jwtSettings["Issuer"]);
Log.Information("  Audience: {Audience}", jwtSettings["Audience"]);
Log.Information("  Key length: {KeyLength} bytes", key.Length);

// Добавление сервисов
Log.Information("Adding services to container");

builder.Services.AddControllers();
Log.Information("Added controllers");

builder.Services.AddEndpointsApiExplorer();
Log.Information("Added endpoints API explorer");

// Настройка Swagger
builder.Services.AddSwaggerGen(options =>
{
    Log.Information("Configuring Swagger");

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebApplication1 API",
        Version = "v1",
        Description = "API for notebooks and notes management"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    Log.Information("Swagger security configuration completed");
});

// Подключаем аутентификацию (JWT)
Log.Information("Configuring JWT authentication");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    Log.Information("Authentication schemes configured: {DefaultAuthenticateScheme}, {DefaultChallengeScheme}",
        JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT authentication failed: {ErrorMessage}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("JWT token validated successfully for user: {UserName}",
                context.Principal?.Identity?.Name ?? "unknown");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Log.Debug("JWT token received");
            return Task.CompletedTask;
        }
    };

    Log.Information("JWT bearer authentication configured with issuer validation: {ValidateIssuer}", true);
});

// Настройка логирования через Serilog
Log.Information("Configuring Serilog logging");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
Log.Information("Serilog configured with Seq sink at http://localhost:5341");

// Настройка базы данных
string connString = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Information("Database connection string: {ConnectionString}",
    string.IsNullOrEmpty(connString) ? "NOT FOUND" : "***" + connString.Substring(Math.Max(0, connString.Length - 10)));

if (string.IsNullOrEmpty(connString))
{
    Log.Error("Database connection string is null or empty");
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(connString);
        Log.Information("Entity Framework SQL Server provider configured");
    });
}

// Настройка Identity
Log.Information("Configuring ASP.NET Core Identity");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Настройки пароля
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Настройки пользователя
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    Log.Information("Identity options configured: Password length={PasswordLength}, Require unique email={RequireUniqueEmail}",
        options.Password.RequiredLength, options.User.RequireUniqueEmail);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

Log.Information("Identity configured with Entity Framework stores and default token providers");

// Регистрируем репозитории
builder.Services.AddScoped<INotebookRepository, NotebookRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
Log.Information("Repositories registered in DI container");

var app = builder.Build();

Log.Information("Application built successfully, configuring middleware");

// Middleware pipeline configuration
if (app.Environment.IsDevelopment())
{
    Log.Information("Development environment detected, enabling Swagger");
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1");
        options.RoutePrefix = "swagger"; // Измените на "swagger" вместо пустой строки
        options.DocumentTitle = "WebApplication1 API";
        Log.Information("Swagger UI configured at /swagger path");
    });
}
else
{
    Log.Information("Production environment detected, Swagger disabled");
}

// Настройка middleware pipeline
Log.Information("Configuring middleware pipeline");

app.UseRouting();
Log.Information("Routing middleware added");

app.UseAuthentication();
Log.Information("Authentication middleware added");

app.UseAuthorization();
Log.Information("Authorization middleware added");

app.MapControllers();
Log.Information("Controllers mapped");

// Диагностика зарегистрированных endpoints
try
{
    var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
    var endpoints = endpointDataSource.Endpoints.ToList();

    Log.Information("=== Registered Endpoints (Total: {EndpointCount}) ===", endpoints.Count);

    foreach (var endpoint in endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            var routePattern = routeEndpoint.RoutePattern.RawText ?? "N/A";
            var httpMethods = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods ??
                            new[] { "ANY" };

            Log.Information("Endpoint: {RoutePattern} - Methods: {HttpMethods} - Display: {DisplayName}",
                routePattern, string.Join(", ", httpMethods), endpoint.DisplayName);
        }
        else
        {
            Log.Information("Endpoint: {DisplayName} (Non-RouteEndpoint)", endpoint.DisplayName);
        }
    }

    Log.Information("=== End of Endpoints List ===");
}
catch (Exception ex)
{
    Log.Error(ex, "Error while logging registered endpoints");
}

// Проверка доступности endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/api", () => "WebApplication1 API is running");

Log.Information("Application startup completed successfully");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Application Name: {ApplicationName}", builder.Environment.ApplicationName);
Log.Information("Content Root Path: {ContentRootPath}", builder.Environment.ContentRootPath);
Log.Information("Web Root Path: {WebRootPath}", builder.Environment.WebRootPath);

Log.Information("Application URLs:");
Log.Information("  - Swagger UI: http://localhost:5042/swagger");
Log.Information("  - API: http://localhost:5042/api");
Log.Information("  - Test endpoint: http://localhost:5042/api/notebooks/test");

app.Run();

Log.Information("Application is shutting down");