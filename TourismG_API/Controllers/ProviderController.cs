using System.Security.Claims;
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
    public class ProviderController(AppDbContext context) : ControllerBase
    {
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var providerId = GetUserId();
            var services = context.ServiceOfferings.AsNoTracking().Where(s => s.ProviderId == providerId);
            var serviceIds = await services.Select(s => s.Id).ToListAsync();
            var bookings = context.Bookings.AsNoTracking().Where(b => serviceIds.Contains(b.ServiceOfferingId));

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
            var services = await context.ServiceOfferings
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
            var service = await context.ServiceOfferings
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

            if (request.PlaceId.HasValue && !await context.Places.AnyAsync(p => p.Id == request.PlaceId.Value))
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

            await context.ServiceOfferings.AddAsync(service);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyService), new { id = service.Id }, service.Id);
        }

        [HttpPut("services/{id:guid}")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpsertServiceRequest request)
        {
            var providerId = GetUserId();
            var service = await context.ServiceOfferings.FirstOrDefaultAsync(s => s.Id == id && s.ProviderId == providerId);
            if (service is null)
            {
                return NotFound();
            }

            if (request.PlaceId.HasValue && !await context.Places.AnyAsync(p => p.Id == request.PlaceId.Value))
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

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("services/{id:guid}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var providerId = GetUserId();
            var service = await context.ServiceOfferings.FirstOrDefaultAsync(s => s.Id == id && s.ProviderId == providerId);
            if (service is null)
            {
                return NotFound();
            }

            context.ServiceOfferings.Remove(service);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var providerId = GetUserId();
            var bookings = await context.Bookings
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
            var booking = await context.Bookings
                .Include(b => b.ServiceOffering)
                .FirstOrDefaultAsync(b => b.Id == id && b.ServiceOffering!.ProviderId == providerId);

            if (booking is null)
            {
                return NotFound();
            }

            booking.Status = request.Status;
            await context.SaveChangesAsync();
            return NoContent();
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
