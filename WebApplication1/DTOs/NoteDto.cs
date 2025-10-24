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
        [Required(ErrorMessage = "Содержание заметки обязательно")]
        [MinLength(1, ErrorMessage = "Заметка не может быть пустой")]
        [MaxLength(10000, ErrorMessage = "Заметка не может превышать 10000 символов")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID блокнота обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID блокнота должен быть положительным числом")]
        public int NotebookId { get; set; }
    }

    public class UpdateNoteDto
    {
        [Required(ErrorMessage = "Содержание заметки обязательно")]
        [MinLength(1, ErrorMessage = "Заметка не может быть пустой")]
        [MaxLength(10000, ErrorMessage = "Заметка не может превышать 10000 символов")]
        public string Content { get; set; } = string.Empty;
    }
}