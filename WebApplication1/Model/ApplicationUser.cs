using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Если хочешь добавить связь с блокнотами
        public virtual ICollection<Notebook> Notebooks { get; set; } = new List<Notebook>();

        // Дополнительные поля пользователя (опционально)
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}