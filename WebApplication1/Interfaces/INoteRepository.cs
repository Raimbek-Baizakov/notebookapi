using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetByIdAsync(int id);
        Task<IEnumerable<Note>> GetByNotebookIdAsync(int notebookId);
        Task<Note> CreateAsync(Note note);
        Task<Note?> UpdateAsync(int id, string content);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> UserOwnsNoteAsync(int noteId, string userId);
    }
}