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
        var requestId = Guid.NewGuid();
        _logger.LogInformation("GET /api/notes/notebook/{NotebookId} started - RequestId: {RequestId}", notebookId, requestId);

        try
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            _logger.LogInformation("User ID extracted: {UserId} for notebook {NotebookId} - RequestId: {RequestId}",
                userId ?? "NULL", notebookId, requestId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized - User ID not found in token for notebook {NotebookId} - RequestId: {RequestId}",
                    notebookId, requestId);
                return Unauthorized(new
                {
                    error = "User ID not found in token",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Querying notes for notebook {NotebookId} and user {UserId} - RequestId: {RequestId}",
                notebookId, userId, requestId);

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

            _logger.LogInformation("Successfully retrieved {NotesCount} notes for notebook {NotebookId} - RequestId: {RequestId}",
                notes.Count, notebookId, requestId);

            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for notebook {NotebookId} - RequestId: {RequestId}",
                notebookId, requestId);
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message,
                requestId = requestId
            });
        }
    }

    // GET: api/notes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetNote(int id)
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation("GET /api/notes/{Id} started - RequestId: {RequestId}", id, requestId);

        try
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            _logger.LogInformation("User ID extracted: {UserId} for note {Id} - RequestId: {RequestId}",
                userId ?? "NULL", id, requestId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized - User ID not found in token for note {Id} - RequestId: {RequestId}",
                    id, requestId);
                return Unauthorized(new
                {
                    error = "User ID not found in token",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Searching for note {Id} for user {UserId} - RequestId: {RequestId}",
                id, userId, requestId);

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
            {
                _logger.LogWarning("Note {Id} not found for user {UserId} - RequestId: {RequestId}",
                    id, userId, requestId);
                return NotFound(new
                {
                    error = $"Note with id {id} not found",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Successfully retrieved note {Id} - RequestId: {RequestId}", id, requestId);
            return note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note {Id} - RequestId: {RequestId}", id, requestId);
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message,
                requestId = requestId
            });
        }
    }

    // POST: api/notes
    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createNoteDto)
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation("POST /api/notes started - RequestId: {RequestId}", requestId);
        _logger.LogInformation("Create note request: {@CreateNoteDto} - RequestId: {RequestId}",
            createNoteDto, requestId);

        try
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            _logger.LogInformation("User ID extracted: {UserId} - RequestId: {RequestId}",
                userId ?? "NULL", requestId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized - User ID not found in token for note creation - RequestId: {RequestId}",
                    requestId);
                return Unauthorized(new
                {
                    error = "User ID not found in token",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Validating notebook {NotebookId} for user {UserId} - RequestId: {RequestId}",
                createNoteDto.NotebookId, userId, requestId);

            var notebook = await _context.Notebooks
                .FirstOrDefaultAsync(n => n.Id == createNoteDto.NotebookId && n.OwnerId == userId);

            if (notebook == null)
            {
                _logger.LogWarning("Notebook {NotebookId} not found or access denied for user {UserId} - RequestId: {RequestId}",
                    createNoteDto.NotebookId, userId, requestId);
                return BadRequest(new
                {
                    error = "Notebook not found or access denied",
                    requestId = requestId
                });
            }

            if (string.IsNullOrWhiteSpace(createNoteDto.Content))
            {
                _logger.LogWarning("Note content is empty for notebook {NotebookId} - RequestId: {RequestId}",
                    createNoteDto.NotebookId, requestId);
                return BadRequest(new
                {
                    error = "Note content is required",
                    requestId = requestId
                });
            }

            var note = new Note
            {
                Content = createNoteDto.Content,
                NotebookId = createNoteDto.NotebookId,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Creating new note in notebook {NotebookId} - RequestId: {RequestId}",
                createNoteDto.NotebookId, requestId);

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var noteDto = new NoteDto
            {
                Id = note.Id,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                NotebookId = note.NotebookId
            };

            _logger.LogInformation("Successfully created note {NoteId} in notebook {NotebookId} - RequestId: {RequestId}",
                note.Id, createNoteDto.NotebookId, requestId);

            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, noteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note - RequestId: {RequestId}", requestId);
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message,
                requestId = requestId
            });
        }
    }

    // PUT: api/notes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteDto updateNoteDto)
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation("PUT /api/notes/{Id} started - RequestId: {RequestId}", id, requestId);
        _logger.LogInformation("Update note request: {@UpdateNoteDto} - RequestId: {RequestId}",
            updateNoteDto, requestId);

        try
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            _logger.LogInformation("User ID extracted: {UserId} for note {Id} - RequestId: {RequestId}",
                userId ?? "NULL", id, requestId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized - User ID not found in token for note update {Id} - RequestId: {RequestId}",
                    id, requestId);
                return Unauthorized(new
                {
                    error = "User ID not found in token",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Searching for note {Id} to update - RequestId: {RequestId}", id, requestId);

            var note = await _context.Notes
                .Include(n => n.Notebook)
                .FirstOrDefaultAsync(n => n.Id == id && n.Notebook!.OwnerId == userId);

            if (note == null)
            {
                _logger.LogWarning("Note {Id} not found for update by user {UserId} - RequestId: {RequestId}",
                    id, userId, requestId);
                return NotFound(new
                {
                    error = $"Note with id {id} not found",
                    requestId = requestId
                });
            }

            if (string.IsNullOrWhiteSpace(updateNoteDto.Content))
            {
                _logger.LogWarning("Note content is empty for note {Id} - RequestId: {RequestId}", id, requestId);
                return BadRequest(new
                {
                    error = "Note content is required",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Updating note {Id} content - RequestId: {RequestId}", id, requestId);

            note.Content = updateNoteDto.Content;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated note {Id} - RequestId: {RequestId}", id, requestId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note {Id} - RequestId: {RequestId}", id, requestId);
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message,
                requestId = requestId
            });
        }
    }

    // DELETE: api/notes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation("DELETE /api/notes/{Id} started - RequestId: {RequestId}", id, requestId);

        try
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            _logger.LogInformation("User ID extracted: {UserId} for note deletion {Id} - RequestId: {RequestId}",
                userId ?? "NULL", id, requestId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized - User ID not found in token for note deletion {Id} - RequestId: {RequestId}",
                    id, requestId);
                return Unauthorized(new
                {
                    error = "User ID not found in token",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Searching for note {Id} to delete - RequestId: {RequestId}", id, requestId);

            var note = await _context.Notes
                .Include(n => n.Notebook)
                .FirstOrDefaultAsync(n => n.Id == id && n.Notebook!.OwnerId == userId);

            if (note == null)
            {
                _logger.LogWarning("Note {Id} not found for deletion by user {UserId} - RequestId: {RequestId}",
                    id, userId, requestId);
                return NotFound(new
                {
                    error = $"Note with id {id} not found",
                    requestId = requestId
                });
            }

            _logger.LogInformation("Deleting note {Id} from notebook {NotebookId} - RequestId: {RequestId}",
                id, note.NotebookId, requestId);

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted note {Id} - RequestId: {RequestId}", id, requestId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {Id} - RequestId: {RequestId}", id, requestId);
            return StatusCode(500, new
            {
                error = "Internal server error",
                details = ex.Message,
                requestId = requestId
            });
        }
    }

    // GET: api/notes/diagnostics
    [HttpGet("diagnostics")]
    [AllowAnonymous]
    public IActionResult Diagnostics()
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation("Diagnostics endpoint called - RequestId: {RequestId}", requestId);

        var diagnostics = new
        {
            timestamp = DateTime.UtcNow,
            requestId = requestId,
            machineName = Environment.MachineName,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            userAuthenticated = User.Identity?.IsAuthenticated,
            userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        _logger.LogInformation("Diagnostics data: {@Diagnostics} - RequestId: {RequestId}",
            diagnostics, requestId);

        return Ok(diagnostics);
    }
}