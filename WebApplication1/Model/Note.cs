namespace WebApplication1.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int NotebookId { get; set; }
        public Notebook? Notebook { get; set; }
    }
}
