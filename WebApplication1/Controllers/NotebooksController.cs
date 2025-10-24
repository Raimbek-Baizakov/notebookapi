using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.DTOs.Notebooks;
using WebApplication1.DTOs.Notes;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotebooksController : ControllerBase
    {
        private readonly INotebookRepository _notebookRepository;
        private readonly ILogger<NotebooksController> _logger;

        public NotebooksController(
            INotebookRepository notebookRepository,
            ILogger<NotebooksController> logger)
        {
            _notebookRepository = notebookRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NotebookDto>>> GetNotebooks()
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("GetNotebooks started - RequestId: {RequestId}", requestId);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User ID not found in token" });
                }

                var notebooks = await _notebookRepository.GetByUserIdAsync(userId);

                var result = notebooks.Select(n => new NotebookDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    OwnerId = n.OwnerId,
                    CreatedAt = n.CreatedAt,
                    NotesCount = n.Notes?.Count ?? 0
                });

                _logger.LogInformation("GetNotebooks completed - RequestId: {RequestId}, Count: {Count}",
                    requestId, result.Count());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNotebooks - RequestId: {RequestId}", requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<NotebookWithNotesDto>> GetNotebook(int id)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("GET /api/notebooks/{Id} started - RequestId: {RequestId}", id, requestId);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User ID not found in token" });
                }

                // Проверяем, принадлежит ли блокнот пользователю
                var userOwnsNotebook = await _notebookRepository.UserOwnsNotebookAsync(id, userId);
                if (!userOwnsNotebook)
                {
                    _logger.LogWarning("Notebook {Id} not found or access denied for user {UserId} - RequestId: {RequestId}",
                        id, userId, requestId);
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                var notebook = await _notebookRepository.GetByIdAsync(id);
                if (notebook == null)
                {
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                var result = new NotebookWithNotesDto
                {
                    Id = notebook.Id,
                    Title = notebook.Title,
                    OwnerId = notebook.OwnerId,
                    CreatedAt = notebook.CreatedAt,
                    Notes = notebook.Notes?.Select(note => new NoteDto
                    {
                        Id = note.Id,
                        Content = note.Content,
                        CreatedAt = note.CreatedAt,
                        NotebookId = note.NotebookId
                    }).ToList() ?? new List<NoteDto>()
                };

                _logger.LogInformation("Successfully retrieved notebook {Id} - RequestId: {RequestId}", id, requestId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notebook {Id} - RequestId: {RequestId}", id, requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<NotebookDto>> CreateNotebook([FromBody] CreateNotebookDto dto)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("CreateNotebook started - RequestId: {RequestId}", requestId);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User ID not found in token" });
                }

                var notebook = new Notebook
                {
                    Title = dto.Title.Trim(),
                    OwnerId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdNotebook = await _notebookRepository.CreateAsync(notebook);

                var result = new NotebookDto
                {
                    Id = createdNotebook.Id,
                    Title = createdNotebook.Title,
                    OwnerId = createdNotebook.OwnerId,
                    CreatedAt = createdNotebook.CreatedAt,
                    NotesCount = 0
                };

                _logger.LogInformation("CreateNotebook completed - RequestId: {RequestId}, NotebookId: {NotebookId}",
                    requestId, createdNotebook.Id);

                return CreatedAtAction(nameof(GetNotebook), new { id = createdNotebook.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateNotebook - RequestId: {RequestId}", requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // PUT: api/notebooks/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateNotebook(int id, [FromBody] UpdateNotebookDto dto)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("PUT /api/notebooks/{Id} started - RequestId: {RequestId}", id, requestId);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User ID not found in token" });
                }

                // Проверяем, принадлежит ли блокнот пользователю
                var userOwnsNotebook = await _notebookRepository.UserOwnsNotebookAsync(id, userId);
                if (!userOwnsNotebook)
                {
                    _logger.LogWarning("Notebook {Id} not found for update by user {UserId} - RequestId: {RequestId}",
                        id, userId, requestId);
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                var updatedNotebook = await _notebookRepository.UpdateAsync(id, dto.Title.Trim());
                if (updatedNotebook == null)
                {
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                _logger.LogInformation("Successfully updated notebook {Id} - RequestId: {RequestId}", id, requestId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notebook {Id} - RequestId: {RequestId}", id, requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // DELETE: api/notebooks/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotebook(int id)
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("DELETE /api/notebooks/{Id} started - RequestId: {RequestId}", id, requestId);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User ID not found in token" });
                }

                // Проверяем, принадлежит ли блокнот пользователю
                var userOwnsNotebook = await _notebookRepository.UserOwnsNotebookAsync(id, userId);
                if (!userOwnsNotebook)
                {
                    _logger.LogWarning("Notebook {Id} not found for deletion by user {UserId} - RequestId: {RequestId}",
                        id, userId, requestId);
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                var deleted = await _notebookRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { error = $"Notebook with id {id} not found" });
                }

                _logger.LogInformation("Successfully deleted notebook {Id} - RequestId: {RequestId}", id, requestId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notebook {Id} - RequestId: {RequestId}", id, requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: api/notebooks/user-info - Для отладки токена
        [HttpGet("user-info")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var requestId = Guid.NewGuid();
            _logger.LogInformation("GET /api/notebooks/user-info started - RequestId: {RequestId}", requestId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            _logger.LogInformation("User Info - RequestId: {RequestId}", requestId);
            _logger.LogInformation("  UserId: {UserId}", userId);
            _logger.LogInformation("  IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("  AuthenticationType: {AuthType}", User.Identity?.AuthenticationType);
            _logger.LogInformation("  Claims count: {ClaimsCount}", claims.Count);

            foreach (var claim in claims)
            {
                _logger.LogInformation("  Claim {Type}: {Value}", claim.Type, claim.Value);
            }

            _logger.LogInformation("User info retrieved successfully - RequestId: {RequestId}", requestId);

            return Ok(new
            {
                userId,
                isAuthenticated = User.Identity?.IsAuthenticated,
                authenticationType = User.Identity?.AuthenticationType,
                claims,
                requestId = requestId
            });
        }

        // GET: api/notebooks/diagnostics - Для полной диагностики
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
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            _logger.LogInformation("Diagnostics data: {@Diagnostics} - RequestId: {RequestId}",
                diagnostics, requestId);

            return Ok(diagnostics);
        }

        // GET: api/notebooks/test - Тестовый endpoint без авторизации
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called");
            return Ok(new
            {
                message = "Controller is working!",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}