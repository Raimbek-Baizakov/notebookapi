using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Notebook
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        // Навигационное свойство к пользователю
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser? Owner { get; set; }

        public DateTime CreatedAt { get; set; }

        // Навигационное свойство к заметкам
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}