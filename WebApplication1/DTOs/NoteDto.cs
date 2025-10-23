using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.Notes
{
    public class NoteDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NotebookId { get; set; }
    }

    public class CreateNoteDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int NotebookId { get; set; }
    }

    public class UpdateNoteDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}