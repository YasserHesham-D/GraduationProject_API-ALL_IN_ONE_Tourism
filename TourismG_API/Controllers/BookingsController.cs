using System.Security.Claims;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController(AppDbContext context) : ControllerBase
    {
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()        // customer bookings
        {
            var userId = GetUserId();

            var serviceBookings = await context.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .Select(b => new CustomerBookingItem(
                    b.Id,
                    "service",
                    b.ServiceOffering!.Title ?? string.Empty,
                    b.ServiceOffering.ImageUrl ?? string.Empty,
                    b.ServiceOffering.LocationName ?? string.Empty,
                    b.BookingDate,
                    null,
                    null,
                    b.Guests,
                    b.TotalPrice,
                    b.Status,
                    b.CreatedAt))
                .ToListAsync();

            var hotelBookings = await context.HotelBookings
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .Select(h => new CustomerBookingItem(
                    h.Id,
                    "hotel",
                    h.Hotel!.Name ?? string.Empty,
                    h.Hotel.ImageUrl ?? string.Empty,
                    h.Hotel.Location ?? string.Empty,
                    null,
                    h.CheckInDate,
                    h.CheckOutDate,
                    h.NumberOfGuests,
                    h.TotalPrice,
                    h.Status,
                    h.CreatedAt))
                .ToListAsync();

            var transportBookings = await context.TransportBookings
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Select(t => new CustomerBookingItem(
                    t.Id,
                    "transport",
                    t.Transport!.Name ?? string.Empty,
                    t.Transport.ImageUrl ?? string.Empty,
                    t.Transport.DepartureLocation + " - " + t.Transport.ArrivalLocation,
                    t.BookingDate,
                    null,
                    null,
                    t.NumberOfSeats,
                    t.TotalPrice,
                    t.Status,
                    t.CreatedAt))
                .ToListAsync();

            var programBookings = await context.ProgramBookings
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new CustomerBookingItem(
                    p.Id,
                    "program",
                    p.Program!.Name ?? string.Empty,
                    p.Program.ImageUrl ?? string.Empty,
                    p.Program.Location ?? string.Empty,
                    p.BookingDate,
                    null,
                    null,
                    p.NumberOfParticipants,
                    p.TotalPrice,
                    p.Status,
                    p.CreatedAt))
                .ToListAsync();

            var guideBookings = await context.GuideBookings
                .AsNoTracking()
                .Where(g => g.UserId == userId)
                .Select(g => new CustomerBookingItem(
                    g.Id,
                    "guide",
                    g.Guide!.FullName ?? string.Empty,
                    g.Guide.ImageUrl ?? string.Empty,
                    g.Guide.Nationality ?? string.Empty,
                    null,
                    g.StartDate,
                    g.EndDate,
                    g.NumberOfPeople,
                    g.TotalPrice,
                    g.Status,
                    g.CreatedAt))
                .ToListAsync();

            var all = serviceBookings
                .Concat(hotelBookings)
                .Concat(transportBookings)
                .Concat(programBookings)
                .Concat(guideBookings)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return Ok(all);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = GetUserId();
            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking is null)
            {
                return NotFound();
            }

            booking.Status = "cancelled";
            await context.SaveChangesAsync();
            return NoContent();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record CustomerBookingItem(
        Guid Id,
        string Type,
        string Title,
        string ImageUrl,
        string LocationName,
        DateTime? BookingDate,
        DateTime? StartDate,
        DateTime? EndDate,
        int Quantity,
        decimal TotalPrice,
        string Status,
        DateTime CreatedAt
    );
}
