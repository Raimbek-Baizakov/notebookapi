using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("Register request started - RequestId: {RequestId}, UserName: {UserName}, Email: {Email}",
                requestId, request.UserName, request.Email);

            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Register validation failed - RequestId: {RequestId}, UserName empty: {UserNameEmpty}, Password empty: {PasswordEmpty}",
                        requestId, string.IsNullOrWhiteSpace(request.UserName), string.IsNullOrWhiteSpace(request.Password));
                    return BadRequest(new { error = "Username and password are required", requestId = requestId });
                }

                _logger.LogInformation("Creating new user - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);

                var user = new ApplicationUser { UserName = request.UserName, Email = request.Email };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("User creation failed - RequestId: {RequestId}, UserName: {UserName}, Errors: {Errors}",
                        requestId, request.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { errors = result.Errors, requestId = requestId });
                }

                _logger.LogInformation("User registered successfully - RequestId: {RequestId}, UserName: {UserName}, UserId: {UserId}",
                    requestId, request.UserName, user.Id);

                return Ok(new
                {
                    Message = "User registered successfully",
                    UserId = user.Id,
                    RequestId = requestId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);
                return StatusCode(500, new
                {
                    error = "Internal server error during registration",
                    details = ex.Message,
                    requestId = requestId
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("Login request started - RequestId: {RequestId}, UserName: {UserName}",
                requestId, request.UserName);

            try
            {
                if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login validation failed - RequestId: {RequestId}, UserName empty: {UserNameEmpty}, Password empty: {PasswordEmpty}",
                        requestId, string.IsNullOrWhiteSpace(request.UserName), string.IsNullOrWhiteSpace(request.Password));
                    return BadRequest(new { error = "Username and password are required", requestId = requestId });
                }

                _logger.LogInformation("Searching for user - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);

                var user = await _userManager.FindByNameAsync(request.UserName);
                if (user == null)
                {
                    _logger.LogWarning("User not found - RequestId: {RequestId}, UserName: {UserName}",
                        requestId, request.UserName);
                    return Unauthorized(new
                    {
                        Message = "Invalid username or password",
                        requestId = requestId
                    });
                }

                _logger.LogInformation("User found - RequestId: {RequestId}, UserName: {UserName}, UserId: {UserId}",
                    requestId, request.UserName, user.Id);

                _logger.LogInformation("Checking password - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Invalid password - RequestId: {RequestId}, UserName: {UserName}",
                        requestId, request.UserName);
                    return Unauthorized(new
                    {
                        Message = "Invalid credentials",
                        requestId = requestId
                    });
                }

                _logger.LogInformation("Password verified successfully - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);

                // Генерация JWT токена
                _logger.LogInformation("Generating JWT token - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);

                var jwtSettings = _configuration.GetSection("Jwt");
                var keyString = jwtSettings["Key"];

                if (string.IsNullOrEmpty(keyString))
                {
                    _logger.LogError("JWT key is not configured - RequestId: {RequestId}", requestId);
                    return StatusCode(500, new
                    {
                        error = "JWT configuration error",
                        requestId = requestId
                    });
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("username", user.UserName ?? "")
                };

                var expiresMinutes = Convert.ToDouble(jwtSettings["ExpireMinutes"] ?? "60");
                var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("JWT token generated successfully - RequestId: {RequestId}, UserName: {UserName}, Token expires: {Expires}",
                    requestId, request.UserName, expires);

                return Ok(new
                {
                    Token = tokenString,
                    Expires = expires,
                    UserName = user.UserName,
                    UserId = user.Id,
                    RequestId = requestId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login - RequestId: {RequestId}, UserName: {UserName}",
                    requestId, request.UserName);
                return StatusCode(500, new
                {
                    error = "Internal server error during login",
                    details = ex.Message,
                    requestId = requestId
                });
            }
        }

        [HttpGet("diagnostics")]
        public IActionResult Diagnostics()
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("Auth diagnostics request - RequestId: {RequestId}", requestId);

            var jwtSettings = _configuration.GetSection("Jwt");
            var diagnostics = new
            {
                RequestId = requestId,
                Timestamp = DateTime.UtcNow,
                JwtConfigured = !string.IsNullOrEmpty(jwtSettings["Key"]),
                JwtIssuer = jwtSettings["Issuer"],
                JwtAudience = jwtSettings["Audience"],
                JwtExpireMinutes = jwtSettings["ExpireMinutes"],
                UserAuthenticated = User.Identity?.IsAuthenticated,
                UserName = User.Identity?.Name
            };

            _logger.LogInformation("Auth diagnostics data - RequestId: {RequestId}, Data: {@Diagnostics}",
                requestId, diagnostics);

            return Ok(diagnostics);
        }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры и подчеркивания")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [StringLength(100, ErrorMessage = "Email не может превышать 100 символов")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и одну цифру")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}