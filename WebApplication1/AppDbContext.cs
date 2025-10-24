using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API для Notebook
            modelBuilder.Entity<Notebook>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(n => n.OwnerId)
                    .IsRequired()
                    .HasMaxLength(450); // Длина ключа Identity пользователя

                entity.Property(n => n.CreatedAt)
                    .IsRequired();

                // Связь с пользователем Identity
                entity.HasOne<ApplicationUser>()
                      .WithMany() // Если у ApplicationUser есть коллекция Notebooks, укажи WithMany(u => u.Notebooks)
                      .HasForeignKey(n => n.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Индекс для быстрого поиска по владельцу
                entity.HasIndex(n => n.OwnerId)
                      .HasDatabaseName("IX_Notebooks_OwnerId");

                // Индекс для названия (если нужно поиск по названию)
                entity.HasIndex(n => n.Title)
                      .HasDatabaseName("IX_Notebooks_Title");
            });

            // Fluent API для Note
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Content)
                    .IsRequired()
                    .HasMaxLength(10000);

                entity.Property(n => n.CreatedAt)
                    .IsRequired();

                entity.Property(n => n.NotebookId)
                    .IsRequired();

                // Связь с блокнотом
                entity.HasOne(n => n.Notebook)
                      .WithMany(n => n.Notes)
                      .HasForeignKey(n => n.NotebookId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Индекс для быстрого поиска по блокноту
                entity.HasIndex(n => n.NotebookId)
                      .HasDatabaseName("IX_Notes_NotebookId");

                // Индекс для даты создания (если нужна сортировка по дате)
                entity.HasIndex(n => n.CreatedAt)
                      .HasDatabaseName("IX_Notes_CreatedAt");
            });

            // Дополнительные настройки для Identity (опционально)
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Можно добавить кастомные настройки для пользователя
                entity.Property(u => u.UserName)
                    .HasMaxLength(256);

                entity.Property(u => u.NormalizedUserName)
                    .HasMaxLength(256);

                entity.Property(u => u.Email)
                    .HasMaxLength(256);

                entity.Property(u => u.NormalizedEmail)
                    .HasMaxLength(256);
            });
        }
    }
}