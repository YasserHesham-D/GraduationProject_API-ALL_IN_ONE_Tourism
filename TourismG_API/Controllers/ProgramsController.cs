using Application.Dtos.Programs;
using Application.Services.ProgramServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Presentation.Services;
using Application.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DbContext;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;
        private readonly ILogger<ProgramsController> _logger;
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public ProgramsController(IProgramService programService, ILogger<ProgramsController> logger, AppDbContext context, IFileUploadService fileUploadService)
        {
            _programService = programService;
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Get all programs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrograms()
        {
            try
            {
                var programs = await _programService.GetAllProgramsAsync();
                return Ok(new { success = true, data = programs });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting programs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get program by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgramById(Guid id)
        {
            try
            {
                var program = await _programService.GetProgramByIdAsync(id);
                if (program == null)
                    return NotFound(new { success = false, message = "Program not found" });

                return Ok(new { success = true, data = program });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting program: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book a program
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookProgram(Guid id, [FromBody] BookProgramRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _programService.BookProgramAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Program booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking program: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/upload-photo")]
        [Authorize]
        public async Task<IActionResult> UploadProgramPhoto(Guid id, [FromForm] UploadPhotoRequest request)
        {
            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id);
            if (program == null)
            {
                return NotFound("Program not found");
            }

            if (request.Photo is null || request.Photo.Length == 0)
            {
                return BadRequest("No file provided");
            }

            try
            {
                if (!_fileUploadService.ValidateFile(request.Photo))
                {
                    return BadRequest("Invalid file. Allowed formats: JPG, PNG, WEBP, GIF, BMP. Max size: 5 MB");
                }

                var photoUrl = await _fileUploadService.UploadFileAsync(request.Photo, "uploads");

                if (!string.IsNullOrEmpty(program.ImageUrl))
                {
                    _fileUploadService.DeleteFile(program.ImageUrl);
                }

                program.ImageUrl = photoUrl;
                await _context.SaveChangesAsync();

                return Ok(new UploadPhotoResponse(true, photoUrl));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new UploadPhotoResponse(false, null, $"Upload failed: {ex.Message}"));
            }
        }
    }
}
