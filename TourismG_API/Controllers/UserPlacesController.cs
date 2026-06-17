using System.Security.Claims;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/user/places")]
    [ApiController]
    [Authorize]
    public class UserPlacesController(AppDbContext context) : ControllerBase
    {
        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedPlaces()
        {
            var userId = GetUserId();
            var places = await context.SavedPlaces
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new UserPlaceItem(
                    s.Place!.Id,
                    s.Place.Name,
                    s.Place.Category,
                    s.Place.LocationName,
                    s.Place.ImageUrl,
                    s.Place.Rating,
                    s.Place.ReviewCount,
                    s.CreatedAt))
                .ToListAsync();

            return Ok(places);
        }

        [HttpPost("saved/{placeId:guid}")]
        public async Task<IActionResult> SavePlace(Guid placeId)
        {
            var userId = GetUserId();
            if (!await context.Places.AnyAsync(p => p.Id == placeId))
            {
                return NotFound("Place not found.");
            }

            var exists = await context.SavedPlaces.AnyAsync(s => s.UserId == userId && s.PlaceId == placeId);
            if (!exists)
            {
                await context.SavedPlaces.AddAsync(new SavedPlace { UserId = userId, PlaceId = placeId });
                await context.SaveChangesAsync();

                return Ok();

            }
            return StatusCode(404,"Already Exist");
        }

        [HttpDelete("saved/{placeId:guid}")]
        public async Task<IActionResult> RemoveSavedPlace(Guid placeId)
        {
            var userId = GetUserId();
            var saved = await context.SavedPlaces.FirstOrDefaultAsync(s => s.UserId == userId && s.PlaceId == placeId);
            if (saved is null)
            {
                return NotFound();
            }

            context.SavedPlaces.Remove(saved);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("visited")]
        public async Task<IActionResult> GetVisitedPlaces()
        {
            var userId = GetUserId();
            var places = await context.VisitedPlaces
                .AsNoTracking()
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.VisitedAt)
                .Select(v => new UserPlaceItem(
                    v.Place!.Id,
                    v.Place.Name,
                    v.Place.Category,
                    v.Place.LocationName,
                    v.Place.ImageUrl,
                    v.Place.Rating,
                    v.Place.ReviewCount,
                    v.VisitedAt))
                .ToListAsync();

            return Ok(places);
        }

        [HttpPost("visited/{placeId:guid}")]
        public async Task<IActionResult> MarkVisited(Guid placeId)
        {
            var userId = GetUserId();
            if (!await context.Places.AnyAsync(p => p.Id == placeId))
            {
                return NotFound("Place not found.");
            }

            var visited = await context.VisitedPlaces.FirstOrDefaultAsync(v => v.UserId == userId && v.PlaceId == placeId);
            if (visited is null)
            {
                await context.VisitedPlaces.AddAsync(new VisitedPlace { UserId = userId, PlaceId = placeId });
            }
            else
            {
                visited.VisitedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return NoContent();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record UserPlaceItem(
        Guid Id,
        string Name,
        string Category,
        string LocationName,
        string ImageUrl,
        decimal Rating,
        int ReviewCount,
        DateTime AddedAt);
}
