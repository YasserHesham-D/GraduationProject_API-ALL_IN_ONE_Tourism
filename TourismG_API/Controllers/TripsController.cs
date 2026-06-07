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
    [Authorize]
    public class TripsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var userId = GetUserId();
            var trips = await context.Trips
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.StartDate)
                .Select(t => new TripSummary(t.Id, t.Title, t.StartDate, t.EndDate, t.Days.Count))
                .ToListAsync();

            return Ok(trips);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTrip(Guid id)
        {
            var userId = GetUserId();
            var trip = await context.Trips
                .AsNoTracking()
                .Where(t => t.Id == id && t.UserId == userId)
                .Select(t => new TripDetails(
                    t.Id,
                    t.Title,
                    t.StartDate,
                    t.EndDate,
                    t.Notes,
                    t.Days.OrderBy(d => d.DayNumber).Select(d => new TripDayDto(
                        d.Id,
                        d.DayNumber,
                        d.Date,
                        d.Activities.OrderBy(a => a.ScheduledAt).Select(a => new TripActivityDto(
                            a.Id,
                            a.Title,
                            a.ScheduledAt,
                            a.Notes,
                            a.PlaceId,
                            a.Place == null ? null : a.Place.Name,
                            a.ServiceOfferingId,
                            a.ServiceOffering == null ? null : a.ServiceOffering.Title)).ToList())).ToList()))
                .FirstOrDefaultAsync();

            return trip is null ? NotFound() : Ok(trip);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Trip title is required.");
            }

            var userId = GetUserId();
            var endDate = request.EndDate ?? request.StartDate;
            if (endDate < request.StartDate)
            {
                return BadRequest("End date cannot be before start date.");
            }

            var trip = new Trip
            {
                UserId = userId,
                Title = request.Title,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Notes = request.Notes
            };

            var days = endDate.DayNumber - request.StartDate.DayNumber + 1;
            for (var i = 0; i < days; i++)
            {
                trip.Days.Add(new TripDay
                {
                    DayNumber = i + 1,
                    Date = request.StartDate.AddDays(i)
                });
            }

            await context.Trips.AddAsync(trip);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, new TripSummary(trip.Id, trip.Title, trip.StartDate, trip.EndDate, trip.Days.Count));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripRequest request)
        {
            var userId = GetUserId();
            var trip = await context.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (trip is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                trip.Title = request.Title;
            }

            trip.Notes = request.Notes;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            var userId = GetUserId();
            var trip = await context.Trips.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (trip is null)
            {
                return NotFound();
            }

            context.Trips.Remove(trip);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{tripId:guid}/days/{dayId:guid}/activities")]
        public async Task<IActionResult> AddActivity(Guid tripId, Guid dayId, [FromBody] CreateTripActivityRequest request)
        {
            var userId = GetUserId();
            var day = await context.TripDays
                .Include(d => d.Trip)
                .FirstOrDefaultAsync(d => d.Id == dayId && d.TripId == tripId && d.Trip!.UserId == userId);

            if (day is null)
            {
                return NotFound();
            }

            if (request.PlaceId.HasValue && !await context.Places.AnyAsync(p => p.Id == request.PlaceId.Value))
            {
                return BadRequest("Place not found.");
            }

            if (request.ServiceOfferingId.HasValue && !await context.ServiceOfferings.AnyAsync(s => s.Id == request.ServiceOfferingId.Value))
            {
                return BadRequest("Service not found.");
            }

            var title = request.Title;
            if (string.IsNullOrWhiteSpace(title) && request.PlaceId.HasValue)
            {
                title = await context.Places.Where(p => p.Id == request.PlaceId.Value).Select(p => p.Name).FirstAsync();
            }

            if (string.IsNullOrWhiteSpace(title) && request.ServiceOfferingId.HasValue)
            {
                title = await context.ServiceOfferings.Where(s => s.Id == request.ServiceOfferingId.Value).Select(s => s.Title).FirstAsync();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Activity title is required.");
            }

            var activity = new TripActivity
            {
                TripDayId = day.Id,
                Title = title,
                PlaceId = request.PlaceId,
                ServiceOfferingId = request.ServiceOfferingId,
                ScheduledAt = request.ScheduledAt,
                Notes = request.Notes
            };

            await context.TripActivities.AddAsync(activity);
            await context.SaveChangesAsync();

            return Ok(new TripActivityDto(activity.Id, activity.Title, activity.ScheduledAt, activity.Notes, activity.PlaceId, null, activity.ServiceOfferingId, null));
        }

        [HttpDelete("{tripId:guid}/activities/{activityId:guid}")]
        public async Task<IActionResult> DeleteActivity(Guid tripId, Guid activityId)
        {
            var userId = GetUserId();
            var activity = await context.TripActivities
                .FirstOrDefaultAsync(a =>
                    a.Id == activityId &&
                    context.TripDays.Any(d => d.Id == a.TripDayId && d.TripId == tripId && d.Trip!.UserId == userId));

            if (activity is null)
            {
                return NotFound();
            }

            context.TripActivities.Remove(activity);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record CreateTripRequest(string Title, DateOnly StartDate, DateOnly? EndDate, string? Notes);
    public record UpdateTripRequest(string? Title, string? Notes);
    public record CreateTripActivityRequest(string? Title, TimeOnly? ScheduledAt, Guid? PlaceId, Guid? ServiceOfferingId, string? Notes);
    public record TripSummary(Guid Id, string Title, DateOnly StartDate, DateOnly? EndDate, int DayCount);
    public record TripDetails(Guid Id, string Title, DateOnly StartDate, DateOnly? EndDate, string? Notes, IReadOnlyCollection<TripDayDto> Days);
    public record TripDayDto(Guid Id, int DayNumber, DateOnly Date, IReadOnlyCollection<TripActivityDto> Activities);
    public record TripActivityDto(Guid Id, string Title, TimeOnly? ScheduledAt, string? Notes, Guid? PlaceId, string? PlaceName, Guid? ServiceOfferingId, string? ServiceTitle);
}
