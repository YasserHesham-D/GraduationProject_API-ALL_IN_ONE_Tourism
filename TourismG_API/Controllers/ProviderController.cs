using System.Security.Claims;
using Application.Dtos.ProviderManagement;
using Application.Dtos.Common;
using Presentation.Services;
using Application.Services.ProviderServices;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Presentation.Controllers
{
    [Route("api/provider")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IProviderService _providerService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;

        public ProviderController(AppDbContext context, IProviderService providerService, IFileUploadService fileUploadService, IConfiguration configuration)
        {
            _context = context;
            _providerService = providerService;
            _fileUploadService = fileUploadService;
            _configuration = configuration;
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var providerId = GetUserId();
            var services = _context.ServiceOfferings.AsNoTracking().Where(s => s.ProviderId == providerId);
            var serviceIds = await services.Select(s => s.Id).ToListAsync();
            var bookings = _context.Bookings.AsNoTracking().Where(b => serviceIds.Contains(b.ServiceOfferingId));

            var totalEarnings = await bookings.Where(b => b.Status != "cancelled").SumAsync(b => b.TotalPrice);
            var totalBookings = await bookings.CountAsync();
            var thisMonthBookings = await bookings.CountAsync(b => b.CreatedAt.Month == DateTime.UtcNow.Month && b.CreatedAt.Year == DateTime.UtcNow.Year);
            var rating = await services.AnyAsync() ? await services.AverageAsync(s => s.Rating) : 0;

            var recentBookings = await bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new ProviderBookingItem(
                    b.Id,
                    b.ServiceOffering!.Title,
                    b.User!.UserName ?? b.User.Email ?? "Customer",
                    b.BookingDate,
                    b.Guests,
                    b.TotalPrice,
                    b.Status))
                .ToListAsync();

            return Ok(new ProviderDashboard(
                totalEarnings,
                totalBookings,
                thisMonthBookings,
                Math.Round(rating, 1),
                recentBookings));
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var providerId = GetUserId();
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.ServiceOffering!.ProviderId == providerId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new ProviderBookingItem(
                    b.Id,
                    b.ServiceOffering!.Title,
                    b.User!.UserName ?? b.User.Email ?? "Customer",
                    b.BookingDate,
                    b.Guests,
                    b.TotalPrice,
                    b.Status))
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPut("bookings/{id:guid}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid id, [FromBody] UpdateBookingStatusRequest request)
        {
            var providerId = GetUserId();
            var booking = await _context.Bookings
                .Include(b => b.ServiceOffering)
                .FirstOrDefaultAsync(b => b.Id == id && b.ServiceOffering!.ProviderId == providerId);


            if (booking is null)
            {
                return Unauthorized("Its NOt your Service To update it ");
            }

            booking.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok("Service Update Success");
        }

        // Provider Request Endpoints
        [HttpPost("request")]
        public async Task<IActionResult> SubmitProviderRequest([FromBody] CreateProviderRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _providerService.SubmitProviderRequestAsync(request, userId);
                return Ok(new { success = true, data = response, message = "Provider request submitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("request/my")]
        public async Task<IActionResult> GetMyProviderRequest()
        {
            try
            {
                var userId = GetUserId();
                var response = await _providerService.GetMyProviderRequestAsync(userId);
                if (response == null)
                    return NotFound(new { success = false, message = "No provider request found" });

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Provider Earnings Endpoint
        [HttpGet("earnings")]
        public async Task<IActionResult> GetProviderEarnings()
        {
            try
            {
                var providerId = GetUserId();
                var earnings = await _providerService.GetProviderEarningsAsync(providerId);
                if (earnings == null)
                    return NotFound(new { success = false, message = "No earnings record found" });

                return Ok(new { success = true, data = earnings });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Provider Booking Management Endpoints
        [HttpPut("bookings/{id:guid}/confirm")]
        public async Task<IActionResult> ConfirmBooking(Guid id)
        {
            try
            {
                var providerId = GetUserId();
                var result = await _providerService.ConfirmBookingAsync(id, providerId);
                if (!result)
                    return NotFound(new { success = false, message = "Booking not found or not authorized" });

                return Ok(new { success = true, message = "Booking confirmed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("places/{id:guid}/upload-photo")]
        public async Task<IActionResult> UploadPlacePhoto(Guid id, [FromForm] UploadPhotoRequest? request)
        {
            var place = await _context.Places.FirstOrDefaultAsync(p => p.Id == id);

            if (place is null)
            {
                return NotFound("Place not found");
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

                // Delete old image if it exists
                if (!string.IsNullOrEmpty(place.ImageUrl))
                {
                    _fileUploadService.DeleteFile(place.ImageUrl);
                }

                place.ImageUrl = photoUrl;
                await _context.SaveChangesAsync();

                return Ok(new UploadPhotoResponse(true, photoUrl));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new UploadPhotoResponse(false, null, $"Upload failed: {ex.Message}"));
            }
        }

        [HttpPut("bookings/{id:guid}/decline")]
        public async Task<IActionResult> DeclineBooking(Guid id)
        {
            try
            {
                var providerId = GetUserId();
                var result = await _providerService.DeclineBookingAsync(id, providerId);
                if (!result)
                    return NotFound(new { success = false, message = "Booking not found or not authorized" });

                return Ok(new { success = true, message = "Booking declined successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("bookings/{id:guid}/complete")]
        public async Task<IActionResult> CompleteBooking(Guid id)
        {
            try
            {
                var providerId = GetUserId();
                var result = await _providerService.CompleteBookingAsync(id, providerId);
                if (!result)
                    return NotFound(new { success = false, message = "Booking not found or not authorized" });

                return Ok(new { success = true, message = "Booking completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("bookings/{id:guid}/contact")]
        public async Task<IActionResult> ContactUserForBooking(Guid id, [FromBody] ProviderContactRequestDto? request)
        {
            try
            {
                var providerId = GetUserId();
                var result = await _providerService.ContactUserAsync(id, providerId, request.Message);
                if (!result)
                    return NotFound(new { success = false, message = "Booking not found or not authorized" });

                return Ok(new { success = true, message = "Message sent to user successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


      

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record ProviderDashboard(decimal TotalEarnings, int TotalBookings, int ThisMonthBookings, decimal Rating, IReadOnlyCollection<ProviderBookingItem> RecentBookings);
    public record ProviderBookingItem(Guid Id, string ServiceTitle, string CustomerName, DateTime BookingDate, int Guests, decimal TotalPrice, string Status);
    public record ProviderServiceItem(Guid Id, string Title, string Category, string Description, decimal Price, string Currency, DateTime StartDateTime, DateTime EndDateTime, string LocationName, string ImageUrl, decimal Rating, int BookingCount, bool IsActive, Guid? PlaceId);
    public record ProviderPlaceItem(Guid Id, string Name, string Category, string City, string Country, string LocationName, string Description, string ImageUrl, string OpeningHours, decimal Rating, int ReviewCount, decimal? PriceFrom, decimal? DistanceKm, decimal? Latitude, decimal? Longitude, bool IsRecommended, bool IsPopular);
    public record UpsertServiceRequest(Guid? PlaceId, string? Title, string? Category, string? Description, decimal Price, string? Currency, DateTime StartDateTime, DateTime EndDateTime, string LocationName, string ImageUrl, decimal Rating, bool? IsActive);
    public record UpdateBookingStatusRequest(string Status);
}
