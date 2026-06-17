using Application.Dtos.Hotels;
using Application.Services.HotelServices;
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
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger, AppDbContext context, IFileUploadService fileUploadService)
        {
            _hotelService = hotelService;
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Get all hotels
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHotels()
        {
            try
            {
                var hotels = await _hotelService.GetAllHotelsAsync();
                return Ok(new { success = true, data = hotels });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting hotels: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get hotel by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(Guid id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound(new { success = false, message = "Hotel not found" });

                return Ok(new { success = true, data = hotel });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting hotel: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book a hotel
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookHotel(Guid id, [FromBody] BookHotelRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _hotelService.BookHotelAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Hotel booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking hotel: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/upload-photo")]
        [Authorize]
        public async Task<IActionResult> UploadHotelPhoto(Guid id, [FromForm] UploadPhotoRequest request)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound("Hotel not found");
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

                if (!string.IsNullOrEmpty(hotel.ImageUrl))
                {
                    _fileUploadService.DeleteFile(hotel.ImageUrl);
                }

                hotel.ImageUrl = photoUrl;
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
