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
            var bookings = await context.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new CustomerBookingItem(
                    b.Id,
                    b.ServiceOffering!.Title,
                    b.ServiceOffering.ImageUrl,
                    b.ServiceOffering.LocationName,
                    b.BookingDate,
                    b.Guests,
                    b.TotalPrice,
                    b.Status))
                .ToListAsync();

            return Ok(bookings);
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

    public record CustomerBookingItem(Guid Id, string ServiceTitle, string ImageUrl, string LocationName, DateTime BookingDate, int Guests, decimal TotalPrice, string Status);
}
