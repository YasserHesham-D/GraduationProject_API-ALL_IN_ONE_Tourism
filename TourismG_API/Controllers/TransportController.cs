using Application.Dtos.Transport;
using Application.Services.TransportServices;
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
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;
        private readonly ILogger<TransportController> _logger;
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;

        public TransportController(ITransportService transportService, ILogger<TransportController> logger, AppDbContext context, IFileUploadService fileUploadService)
        {
            _transportService = transportService;
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Get all transport options
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransports()
        {
            try
            {
                var transports = await _transportService.GetAllTransportsAsync();
                return Ok(new { success = true, data = transports });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transports: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get transport by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransportById(Guid id)
        {
            try
            {
                var transport = await _transportService.GetTransportByIdAsync(id);
                if (transport == null)
                    return NotFound(new { success = false, message = "Transport not found" });

                return Ok(new { success = true, data = transport });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transport: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book transport
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookTransport(Guid id, [FromBody] BookTransportRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _transportService.BookTransportAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Transport booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking transport: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/upload-photo")]
        [Authorize]
        public async Task<IActionResult> UploadTransportPhoto(Guid id, [FromForm] UploadPhotoRequest request)
        {
            var transport = await _context.Transports.FirstOrDefaultAsync(t => t.Id == id);
            if (transport == null)
            {
                return NotFound("Transport not found");
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

                if (!string.IsNullOrEmpty(transport.ImageUrl))
                {
                    _fileUploadService.DeleteFile(transport.ImageUrl);
                }

                transport.ImageUrl = photoUrl;
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
        public async Task<IActionResult> CreateTransport([FromBody] CreateTransportRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var transport = new Domain.Models.Transport
            {
                ProviderId = providerId,
                Name = request.Name ?? string.Empty,
                Type = request.Type ?? string.Empty,
                Description = request.Description ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                DepartureLocation = request.DepartureLocation ?? string.Empty,
                ArrivalLocation = request.ArrivalLocation ?? string.Empty,
                DepartureTime = request.DepartureTime ?? string.Empty,
                ArrivalTime = request.ArrivalTime ?? string.Empty,
                Price = request.Price,
                TotalCapacity = request.TotalCapacity,
                AvailableSeats = request.TotalCapacity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Transports.AddAsync(transport);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransportById), new { id = transport.Id }, transport.Id);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateTransport(Guid id, [FromBody] CreateTransportRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transport = await _context.Transports.FirstOrDefaultAsync(t => t.Id == id && t.ProviderId == providerId);
            if (transport is null)
                return Unauthorized("Not your transport or not found");

            transport.Name = request.Name ?? transport.Name;
            transport.Type = request.Type ?? transport.Type;
            transport.Description = request.Description ?? transport.Description;
            transport.ImageUrl = request.ImageUrl ?? transport.ImageUrl;
            transport.DepartureLocation = request.DepartureLocation ?? transport.DepartureLocation;
            transport.ArrivalLocation = request.ArrivalLocation ?? transport.ArrivalLocation;
            transport.DepartureTime = request.DepartureTime ?? transport.DepartureTime;
            transport.ArrivalTime = request.ArrivalTime ?? transport.ArrivalTime;
            transport.Price = request.Price;
            transport.TotalCapacity = request.TotalCapacity;
            transport.AvailableSeats = request.TotalCapacity;
            transport.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteTransport(Guid id)
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transport = await _context.Transports.FirstOrDefaultAsync(t => t.Id == id && t.ProviderId == providerId);
            if (transport is null)
                return Unauthorized("Not your transport or not found");

            if (!string.IsNullOrEmpty(transport.ImageUrl))
            {
                _fileUploadService.DeleteFile(transport.ImageUrl);
            }

            _context.Transports.Remove(transport);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyTransports()
        {
            var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transports = await _context.Transports
                .AsNoTracking()
                .Where(t => t.ProviderId == providerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new Application.Dtos.Transport.TransportResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Type = t.Type,
                    Description = t.Description,
                    ImageUrl = t.ImageUrl,
                    DepartureLocation = t.DepartureLocation,
                    ArrivalLocation = t.ArrivalLocation,
                    DepartureTime = t.DepartureTime,
                    ArrivalTime = t.ArrivalTime,
                    Price = t.Price,
                    AvailableSeats = t.AvailableSeats,
                    TotalCapacity = t.TotalCapacity,
                    Rating = t.Rating,
                    ReviewCount = t.ReviewCount
                })
                .ToListAsync();

            return Ok(transports);
        }
    }
}
