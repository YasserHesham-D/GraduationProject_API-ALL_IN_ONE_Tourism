using Application.Dtos.Programs;
using Application.Services.ProgramServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Presentation.Services;
using Application.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DbContext;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

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
        public async Task<IActionResult> GetAllPrograms()
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

                // Build absolute URL
                var baseUrl = _configuration["PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var absoluteUrl = $"{baseUrl}{photoUrl}";

                if (!string.IsNullOrEmpty(program.ImageUrl))
                {
                    _fileUploadService.DeleteFile(program.ImageUrl);
                }

                program.ImageUrl = photoUrl;
                await _context.SaveChangesAsync();

                return Ok(new UploadPhotoResponse(true, absoluteUrl));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new UploadPhotoResponse(false, null, $"Upload failed: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var program = new Domain.Models.Program
            {
                ProviderId = providerId,
                Name = request.Name ?? string.Empty,
                Description = request.Description ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                Category = request.Category ?? string.Empty,
                Location = request.Location ?? string.Empty,
                City = request.City ?? string.Empty,
                Country = request.Country ?? "Egypt",
                Price = request.Price,
                Duration = request.Duration,
                MaxParticipants = request.MaxParticipants,
                AvailableSpots = request.MaxParticipants,
                IncludedServices = request.IncludedServices ?? string.Empty,
                StartDate = request.StartDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Programs.AddAsync(program);
            await _context.SaveChangesAsync();

            return Ok("Program Added success");
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] CreateProgramRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id && p.ProviderId == providerId);
            if (program is null)
                return Unauthorized("Not your program or not found");

            program.Name = request.Name ?? program.Name;
            program.Description = request.Description ?? program.Description;
            program.ImageUrl = request.ImageUrl ?? program.ImageUrl;
            program.Category = request.Category ?? program.Category;
            program.Location = request.Location ?? program.Location;
            program.City = request.City ?? program.City;
            program.Country = request.Country ?? program.Country;
            program.Price = request.Price;
            program.Duration = request.Duration;
            program.MaxParticipants = request.MaxParticipants;
            program.AvailableSpots = request.MaxParticipants;
            program.IncludedServices = request.IncludedServices ?? program.IncludedServices;
            program.StartDate = request.StartDate;
            program.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id && p.ProviderId == providerId);
            if (program is null)
                return Unauthorized("Not your program or not found");

            if (!string.IsNullOrEmpty(program.ImageUrl))
            {
                _fileUploadService.DeleteFile(program.ImageUrl);
            }

            _context.Programs.Remove(program);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPrograms()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var programs = await _context.Programs
                .AsNoTracking()
                .Where(p => p.ProviderId == providerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new Application.Dtos.Programs.ProgramResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    Location = p.Location,
                    City = p.City,
                    Country = p.Country,
                    Price = p.Price,
                    Duration = p.Duration,
                    MaxParticipants = p.MaxParticipants,
                    AvailableSpots = p.AvailableSpots,
                    IncludedServices = p.IncludedServices,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    StartDate = p.StartDate
                })
                .ToListAsync();

            return Ok(programs);
        }
    }
}
