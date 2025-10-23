using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1;
using WebApplication1.DTOs.Notebooks;
using WebApplication1.DTOs.Notes;
using WebApplication1.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotebooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotebooksController> _logger;

    public NotebooksController(AppDbContext context, ILogger<NotebooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/notebooks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotebookDto>>> GetNotebooks()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notebooks = await _context.Notebooks
            .Where(n => n.OwnerId == userId)
            .Select(n => new NotebookDto
            {
                Id = n.Id,
                Title = n.Title,
                OwnerId = n.OwnerId,
                CreatedAt = n.CreatedAt,
                NotesCount = n.Notes!.Count
            })
            .ToListAsync();

        return Ok(notebooks);
    }

    // GET: api/notebooks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<NotebookWithNotesDto>> GetNotebook(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notebook = await _context.Notebooks
            .Include(n => n.Notes)
            .Where(n => n.Id == id && n.OwnerId == userId)
            .Select(n => new NotebookWithNotesDto
            {
                Id = n.Id,
                Title = n.Title,
                OwnerId = n.OwnerId,
                CreatedAt = n.CreatedAt,
                Notes = n.Notes!.Select(note => new NoteDto
                {
                    Id = note.Id,
                    Content = note.Content,
                    CreatedAt = note.CreatedAt,
                    NotebookId = note.NotebookId
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (notebook == null)
            return NotFound();

        return notebook;
    }

    // POST: api/notebooks
    [HttpPost]
    public async Task<ActionResult<NotebookDto>> CreateNotebook(CreateNotebookDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var notebook = new Notebook
        {
            Title = dto.Title,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notebooks.Add(notebook);
        await _context.SaveChangesAsync();

        var result = new NotebookDto
        {
            Id = notebook.Id,
            Title = notebook.Title,
            OwnerId = notebook.OwnerId,
            CreatedAt = notebook.CreatedAt,
            NotesCount = 0
        };

        return CreatedAtAction(nameof(GetNotebook), new { id = notebook.Id }, result);
    }

    // PUT: api/notebooks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNotebook(int id, UpdateNotebookDto dto)
    {
        // ИСПРАВЛЕНО
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notebook = await _context.Notebooks
            .FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId);

        if (notebook == null)
            return NotFound();

        notebook.Title = dto.Title;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/notebooks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotebook(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notebook = await _context.Notebooks
            .Include(n => n.Notes)
            .FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId);

        if (notebook == null)
            return NotFound();

        _context.Notes.RemoveRange(notebook.Notes);
        _context.Notebooks.Remove(notebook);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/notebooks/test
    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new { message = "Контроллер работает!" });
    }
}