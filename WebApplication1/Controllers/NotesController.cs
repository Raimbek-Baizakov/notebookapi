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
public class NotesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotesController> _logger;

    public NotesController(AppDbContext context, ILogger<NotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/notes/notebook/5
    [HttpGet("notebook/{notebookId}")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotesByNotebook(int notebookId)
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        var notes = await _context.Notes
            .Include(n => n.Notebook)
            .Where(n => n.NotebookId == notebookId && n.Notebook!.OwnerId == userId)
            .Select(n => new NoteDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                NotebookId = n.NotebookId
            })
            .ToListAsync();

        return Ok(notes);
    }

    // GET: api/notes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetNote(int id)
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        var note = await _context.Notes
            .Include(n => n.Notebook)
            .Where(n => n.Id == id && n.Notebook!.OwnerId == userId)
            .Select(n => new NoteDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt,
                NotebookId = n.NotebookId
            })
            .FirstOrDefaultAsync();

        if (note == null)
            return NotFound();

        return note;
    }

    // POST: api/notes
    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createNoteDto)
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        var notebook = await _context.Notebooks
            .FirstOrDefaultAsync(n => n.Id == createNoteDto.NotebookId && n.OwnerId == userId);

        if (notebook == null)
            return BadRequest("Notebook not found or access denied");

        var note = new Note
        {
            Content = createNoteDto.Content,
            NotebookId = createNoteDto.NotebookId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        var noteDto = new NoteDto
        {
            Id = note.Id,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            NotebookId = note.NotebookId
        };

        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, noteDto);
    }

    // PUT: api/notes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteDto updateNoteDto)
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        var note = await _context.Notes
            .Include(n => n.Notebook)
            .FirstOrDefaultAsync(n => n.Id == id && n.Notebook!.OwnerId == userId);

        if (note == null)
            return NotFound();

        note.Content = updateNoteDto.Content;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/notes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        var note = await _context.Notes
            .Include(n => n.Notebook)
            .FirstOrDefaultAsync(n => n.Id == id && n.Notebook!.OwnerId == userId);

        if (note == null)
            return NotFound();

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}