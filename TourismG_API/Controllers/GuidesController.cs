using Application.Dtos.Guides;
using Application.Services.GuideServices;
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
    public class GuidesController : ControllerBase
    {
        private readonly IGuideService _guideService;
        private readonly ILogger<GuidesController> _logger;
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;

        public GuidesController(IGuideService guideService, ILogger<GuidesController> logger, AppDbContext context, IFileUploadService fileUploadService, IConfiguration configuration)
        {
            _guideService = guideService;
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all available guides
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGuides()
        {
            try
            {
                var guides = await _guideService.GetAllGuidesAsync();
                return Ok(new { success = true, data = guides });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting guides: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get guide by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuideById(Guid id)
        {
            try
            {
                var guide = await _guideService.GetGuideByIdAsync(id);
                if (guide == null)
                    return NotFound(new { success = false, message = "Guide not found" });

                return Ok(new { success = true, data = guide });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting guide: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book a guide
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookGuide(Guid id, [FromBody] BookGuideRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _guideService.BookGuideAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Guide booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking guide: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/upload-photo")]
        [Authorize]
        public async Task<IActionResult> UploadGuidePhoto(Guid id, [FromForm] UploadPhotoRequest request)
        {
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == id);
            if (guide == null)
            {
                return NotFound("Guide not found");
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

                if (!string.IsNullOrEmpty(guide.ImageUrl))
                {
                    _fileUploadService.DeleteFile(guide.ImageUrl);
                }

                guide.ImageUrl = photoUrl;
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
        public async Task<IActionResult> CreateGuide([FromBody] CreateGuideRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var guide = new Domain.Models.Guide
            {
                ProviderId = providerId,
                FullName = request.FullName ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                Email = request.Email ?? string.Empty,
                Description = request.Description ?? string.Empty,
                Nationality = request.Nationality ?? string.Empty,
                Languages = request.Languages ?? string.Empty,
                Specialization = request.Specialization ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                Bio = request.Bio ?? string.Empty,
                PricePerDay = request.PricePerDay,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Guides.AddAsync(guide);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuideById), new { id = guide.Id }, guide.Id);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateGuide(Guid id, [FromBody] CreateGuideRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == id && g.ProviderId == providerId);
            if (guide is null)
                return Unauthorized("Not your guide or not found");

            guide.FullName = request.FullName ?? guide.FullName;
            guide.PhoneNumber = request.PhoneNumber ?? guide.PhoneNumber;
            guide.Email = request.Email ?? guide.Email;
            guide.Description = request.Description ?? guide.Description;
            guide.Nationality = request.Nationality ?? guide.Nationality;
            guide.Languages = request.Languages ?? guide.Languages;
            guide.Specialization = request.Specialization ?? guide.Specialization;
            guide.ImageUrl = request.ImageUrl ?? guide.ImageUrl;
            guide.Bio = request.Bio ?? guide.Bio;
            guide.PricePerDay = request.PricePerDay;
            guide.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteGuide(Guid id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == id && g.ProviderId == providerId);
            if (guide is null)
                return Unauthorized("Not your guide or not found");

            if (!string.IsNullOrEmpty(guide.ImageUrl))
            {
                _fileUploadService.DeleteFile(guide.ImageUrl);
            }

            _context.Guides.Remove(guide);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyGuides()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guides = await _context.Guides
                .AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new Application.Dtos.Guides.GuideResponse
                {
                    Id = g.Id,
                    FullName = g.FullName,
                    PhoneNumber = g.PhoneNumber,
                    Email = g.Email,
                    Description = g.Description,
                    Nationality = g.Nationality,
                    Languages = g.Languages,
                    Specialization = g.Specialization,
                    ImageUrl = g.ImageUrl,
                    Bio = g.Bio,
                    PricePerDay = g.PricePerDay,
                    Rating = g.Rating,
                    ReviewCount = g.ReviewCount,
                    IsAvailable = g.IsAvailable
                })
                .ToListAsync();

            return Ok(guides);
        }
    }
}
