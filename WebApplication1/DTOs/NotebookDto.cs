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
        [Required(ErrorMessage = "Название блокнота обязательно")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Название должно быть от 1 до 100 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\.\-_]+$", ErrorMessage = "Название может содержать только буквы, цифры, пробелы и символы .-_")]
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateNotebookDto
    {
        [Required(ErrorMessage = "Название блокнота обязательно")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Название должно быть от 1 до 100 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\.\-_]+$", ErrorMessage = "Название может содержать только буквы, цифры, пробелы и символы .-_")]
        public string Title { get; set; } = string.Empty;
    }
}