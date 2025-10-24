using Microsoft.EntityFrameworkCore;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class NotebookRepository : INotebookRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotebookRepository> _logger;

        public NotebookRepository(AppDbContext context, ILogger<NotebookRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notebook?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting notebook by ID: {NotebookId}", id);
            return await _context.Notebooks
                .Include(n => n.Notes)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Notebook>> GetByUserIdAsync(string userId)
        {
            _logger.LogInformation("Getting notebooks for user: {UserId}", userId);
            return await _context.Notebooks
                .Where(n => n.OwnerId == userId)
                .Include(n => n.Notes)
                .ToListAsync();
        }

        public async Task<Notebook> CreateAsync(Notebook notebook)
        {
            _logger.LogInformation("Creating new notebook for user: {UserId}", notebook.OwnerId);

            _context.Notebooks.Add(notebook);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notebook created with ID: {NotebookId}", notebook.Id);
            return notebook;
        }

        public async Task<Notebook?> UpdateAsync(int id, string title)
        {
            _logger.LogInformation("Updating notebook: {NotebookId}", id);

            var notebook = await _context.Notebooks.FindAsync(id);
            if (notebook == null)
            {
                _logger.LogWarning("Notebook not found for update: {NotebookId}", id);
                return null;
            }

            notebook.Title = title;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notebook updated: {NotebookId}", id);
            return notebook;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting notebook: {NotebookId}", id);

            var notebook = await _context.Notebooks
                .Include(n => n.Notes)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notebook == null)
            {
                _logger.LogWarning("Notebook not found for deletion: {NotebookId}", id);
                return false;
            }

            // Каскадное удаление заметок
            if (notebook.Notes?.Any() == true)
            {
                _context.Notes.RemoveRange(notebook.Notes);
                _logger.LogInformation("Removed {NotesCount} notes from notebook {NotebookId}",
                    notebook.Notes.Count, id);
            }

            _context.Notebooks.Remove(notebook);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notebook deleted: {NotebookId}", id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Notebooks.AnyAsync(n => n.Id == id);
        }

        public async Task<bool> UserOwnsNotebookAsync(int notebookId, string userId)
        {
            return await _context.Notebooks
                .AnyAsync(n => n.Id == notebookId && n.OwnerId == userId);
        }
    }
}