using WebApplication1.DTOs.Notebooks;
using WebApplication1.Models;

namespace WebApplication1.Interfaces
{
    public interface INotebookRepository
    {
        Task<Notebook?> GetByIdAsync(int id);
        Task<IEnumerable<Notebook>> GetByUserIdAsync(string userId);
        Task<Notebook> CreateAsync(Notebook notebook);
        Task<Notebook?> UpdateAsync(int id, string title);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> UserOwnsNotebookAsync(int notebookId, string userId);
    }
}