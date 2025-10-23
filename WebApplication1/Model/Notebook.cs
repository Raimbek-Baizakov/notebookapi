namespace WebApplication1.Models
{
    public class Notebook
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }
        public ICollection<Note>? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    }
}