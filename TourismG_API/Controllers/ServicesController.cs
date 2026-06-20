using System.Security.Claims;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController(AppDbContext context) : ControllerBase
    {
        //[HttpGet]
        //public async Task<IActionResult> GetServices([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        //{
        //    page = Math.Max(page, 1);
        //    pageSize = Math.Clamp(pageSize, 1, 50);

        //    var query = context.ServiceOfferings.AsNoTracking().Where(s => s.IsActive);

        //    if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        //    {
        //        query = query.Where(s => s.Category == category);
        //    }

        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        query = query.Where(s =>
        //            s.Title.Contains(search) ||
        //            s.Category.Contains(search) ||
        //            s.LocationName.Contains(search));
        //    }

        //    var totalCount = await query.CountAsync();
        //    var services = await query
        //        .OrderByDescending(s => s.Rating)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(s => new ServiceDetailsDto(
        //            s.Id,
        //            s.Title,
        //            s.Category,
        //            s.Description,
        //            s.Price,
        //            s.Currency,
        //            s.StartDateTime,
        //            s.EndDateTime,
        //            s.LocationName,
        //            s.ImageUrl,
        //            s.Rating,
        //            s.BookingCount,
        //            s.PlaceId,
        //            s.Place == null ? null : s.Place.Name))
        //        .ToListAsync();

        //    return Ok(new PagedResponse<ServiceDetailsDto>(services, page, pageSize, totalCount));
        //}

        //[HttpGet("{id:guid}")]
        //public async Task<IActionResult> GetService(Guid id)
        //{
        //    var service = await context.ServiceOfferings
        //        .AsNoTracking()
        //        .Where(s => s.Id == id && s.IsActive)
        //        .Select(s => new ServiceDetailsDto(
        //            s.Id,
        //            s.Title,
        //            s.Category,
        //            s.Description,
        //            s.Price,
        //            s.Currency,
        //            s.StartDateTime,
        //            s.EndDateTime,
        //            s.LocationName,
        //            s.ImageUrl,
        //            s.Rating,
        //            s.BookingCount,
        //            s.PlaceId,
        //            s.Place == null ? null : s.Place.Name))
        //        .FirstOrDefaultAsync();

        //    return service is null ? NotFound() : Ok(service);
        //}

        //[HttpPost("{id:guid}/book")]
        //[Authorize]
        //public async Task<IActionResult> BookService(Guid id, [FromBody] CreateBookingRequest request)
        //{
        //    var service = await context.ServiceOfferings.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        //    if (service is null)
        //    {
        //        return NotFound();
        //    }

        //    var guests = Math.Max(request.Guests, 1);
        //    var booking = new Booking
        //    {
        //        UserId = GetUserId(),
        //        ServiceOfferingId = service.Id,
        //        BookingDate = request.BookingDate,
        //        Guests = guests,
        //        TotalPrice = service.Price * guests,
        //        Status = "pending"
        //    };

        //    service.BookingCount += 1;
        //    await context.Bookings.AddAsync(booking);
        //    await context.SaveChangesAsync();

        //    return Ok(new BookingDto(booking.Id, service.Title, booking.BookingDate, booking.Guests, booking.TotalPrice, booking.Status));
        //}

        //private string GetUserId()
        //{
        //    return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        //}
    }

    public record ServiceDetailsDto(Guid Id, string Title, string Category, string Description, decimal Price, string Currency, DateTime StartDateTime, DateTime EndDateTime, string LocationName, string ImageUrl, decimal Rating, int BookingCount, Guid? PlaceId, string? PlaceName);
    public record CreateBookingRequest(DateTime BookingDate, int Guests);
    public record BookingDto(Guid Id, string ServiceTitle, DateTime BookingDate, int Guests, decimal TotalPrice, string Status);
}
