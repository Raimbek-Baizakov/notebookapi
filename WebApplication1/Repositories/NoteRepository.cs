using Microsoft.EntityFrameworkCore;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NoteRepository> _logger;

        public NoteRepository(AppDbContext context, ILogger<NoteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting note by ID: {NoteId}", id);
            return await _context.Notes
                .Include(n => n.Notebook)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Note>> GetByNotebookIdAsync(int notebookId)
        {
            _logger.LogInformation("Getting notes for notebook: {NotebookId}", notebookId);
            return await _context.Notes
                .Where(n => n.NotebookId == notebookId)
                .ToListAsync();
        }

        public async Task<Note> CreateAsync(Note note)
        {
            _logger.LogInformation("Creating new note for notebook: {NotebookId}", note.NotebookId);

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note created with ID: {NoteId}", note.Id);
            return note;
        }

        public async Task<Note?> UpdateAsync(int id, string content)
        {
            _logger.LogInformation("Updating note: {NoteId}", id);

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                _logger.LogWarning("Note not found for update: {NoteId}", id);
                return null;
            }

            note.Content = content;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note updated: {NoteId}", id);
            return note;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting note: {NoteId}", id);

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                _logger.LogWarning("Note not found for deletion: {NoteId}", id);
                return false;
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note deleted: {NoteId}", id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Notes.AnyAsync(n => n.Id == id);
        }

        public async Task<bool> UserOwnsNoteAsync(int noteId, string userId)
        {
            return await _context.Notes
                .Include(n => n.Notebook)
                .AnyAsync(n => n.Id == noteId && n.Notebook!.OwnerId == userId);
        }
    }
}