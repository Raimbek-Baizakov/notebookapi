using System.ComponentModel.DataAnnotations;
using WebApplication1.DTOs.Notes;

namespace WebApplication1.DTOs.Notebooks
{
    public class NotebookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NotesCount { get; set; }
    }

    public class NotebookWithNotesDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<NoteDto> Notes { get; set; } = new();
    }

    public class CreateNotebookDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateNotebookDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
    }
}