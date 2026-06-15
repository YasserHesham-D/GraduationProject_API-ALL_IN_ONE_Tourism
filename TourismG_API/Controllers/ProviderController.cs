using System.Security.Claims;
using Application.Dtos.ProviderManagement;
using Application.Services.ProviderServices;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/provider")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IProviderService _providerService;

        public ProviderController(AppDbContext context, IProviderService providerService)
        {
            _context = context;
            _providerService = providerService;
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

        [HttpGet("services")]
        public async Task<IActionResult> GetMyServices()
        {
            var providerId = GetUserId();
            var services = await _context.ServiceOfferings
                .AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ProviderServiceItem(
                    s.Id,
                    s.Title,
                    s.Category,
                    s.Description,
                    s.Price,
                    s.Currency,
                    s.Duration,
                    s.LocationName,
                    s.ImageUrl,
                    s.Availability,
                    s.Rating,
                    s.BookingCount,
                    s.IsActive,
                    s.PlaceId))
                .ToListAsync();

            return Ok(services);
        }

        [HttpGet("services/{id:guid}")]
        public async Task<IActionResult> GetMyService(Guid id)
        {
            var providerId = GetUserId();
            var service = await _context.ServiceOfferings
                .AsNoTracking()
                .Where(s => s.Id == id && s.ProviderId == providerId)
                .Select(s => new ProviderServiceItem(
                    s.Id,
                    s.Title,
                    s.Category,
                    s.Description,
                    s.Price,
                    s.Currency,
                    s.Duration,
                    s.LocationName,
                    s.ImageUrl,
                    s.Availability,
                    s.Rating,
                    s.BookingCount,
                    s.IsActive,
                    s.PlaceId))
                .FirstOrDefaultAsync();

            return service is null ? NotFound() : Ok(service);
        }

        [HttpPost("services")]
        public async Task<IActionResult> AddService([FromBody] UpsertServiceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Service title is required.");
            }

            if (request.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (request.PlaceId.HasValue && !await _context.Places.AnyAsync(p => p.Id == request.PlaceId.Value))
            {
                return BadRequest("Place not found.");
            }

            var service = new ServiceOffering
            {
                ProviderId = GetUserId(),
                PlaceId = request.PlaceId,
                Title = request.Title,
                Category = request.Category,
                Description = request.Description,
                Price = request.Price,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EGP" : request.Currency,
                Duration = request.Duration,
                LocationName = request.LocationName,
                ImageUrl = request.ImageUrl,
                Availability = request.Availability,
                Rating = request.Rating,
                IsActive = request.IsActive ?? true
            };

            await _context.ServiceOfferings.AddAsync(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyService), new { id = service.Id }, service.Id);
        }

        [HttpPut("services/{id:guid}")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpsertServiceRequest request)
        {
            var providerId = GetUserId();
            var service = await _context.ServiceOfferings.FirstOrDefaultAsync(s => s.Id == id && s.ProviderId == providerId);
            if (service is null)
            {
                return NotFound();
            }

            if (request.PlaceId.HasValue && !await _context.Places.AnyAsync(p => p.Id == request.PlaceId.Value))
            {
                return BadRequest("Place not found.");
            }

            service.PlaceId = request.PlaceId;
            service.Title = request.Title;
            service.Category = request.Category;
            service.Description = request.Description;
            service.Price = request.Price;
            service.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EGP" : request.Currency;
            service.Duration = request.Duration;
            service.LocationName = request.LocationName;
            service.ImageUrl = request.ImageUrl;
            service.Availability = request.Availability;
            service.Rating = request.Rating;
            service.IsActive = request.IsActive ?? service.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("services/{id:guid}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var providerId = GetUserId();
            var service = await _context.ServiceOfferings.FirstOrDefaultAsync(s => s.Id == id && s.ProviderId == providerId);
            if (service is null)
            {
                return NotFound();
            }

            _context.ServiceOfferings.Remove(service);
            await _context.SaveChangesAsync();
            return NoContent();
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
                return NotFound();
            }

            booking.Status = request.Status;
            await _context.SaveChangesAsync();
            return NoContent();
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
        public async Task<IActionResult> ContactUserForBooking(Guid id, [FromBody] ProviderContactRequestDto request)
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
    public record ProviderServiceItem(Guid Id, string Title, string Category, string Description, decimal Price, string Currency, string Duration, string LocationName, string ImageUrl, string Availability, decimal Rating, int BookingCount, bool IsActive, Guid? PlaceId);
    public record UpsertServiceRequest(Guid? PlaceId, string Title, string Category, string Description, decimal Price, string? Currency, string Duration, string LocationName, string ImageUrl, string Availability, decimal Rating, bool? IsActive);
    public record UpdateBookingStatusRequest(string Status);
}
