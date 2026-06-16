using Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> SearchAll([FromQuery] string query, [FromQuery] int take = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required.");
            }

            take = Math.Clamp(take, 1, 30);

            var places = await context.Places
                .AsNoTracking()
                .Where(p => p.Name.Contains(query) || p.Category.Contains(query) || p.City.Contains(query) || p.LocationName.Contains(query))
                .OrderByDescending(p => p.Rating)
                .Take(take)
                .Select(p => new SearchItem(p.Id, "place", p.Name, p.LocationName, p.ImageUrl, p.Rating))
                .ToListAsync();

            var services = await context.ServiceOfferings
                .AsNoTracking()
                .Where(s => s.IsActive && (s.Title.Contains(query) || s.Category.Contains(query) || s.LocationName.Contains(query)))
                .OrderByDescending(s => s.Rating)
                .Take(take)
                .Select(s => new SearchItem(s.Id, "service", s.Title, s.LocationName, s.ImageUrl, s.Rating))
                .ToListAsync();

            var hotels = await context.Hotels
                .AsNoTracking()
                .Where(h => h.Name.Contains(query) || h.Location.Contains(query) || h.City.Contains(query) || h.Description.Contains(query))
                .OrderByDescending(h => h.Rating)
                .Take(take)
                .Select(h => new SearchItem(h.Id, "hotel", h.Name, h.Location, h.ImageUrl, h.Rating))
                .ToListAsync();

            var transports = await context.Transports
                .AsNoTracking()
                .Where(t => t.Name.Contains(query) || t.Type.Contains(query) || t.DepartureLocation.Contains(query) || t.ArrivalLocation.Contains(query))
                .OrderByDescending(t => t.Rating)
                .Take(take)
                .Select(t => new SearchItem(t.Id, "transport", t.Name, t.DepartureLocation, t.ImageUrl, t.Rating))
                .ToListAsync();

            var programs = await context.Programs
                .AsNoTracking()
                .Where(p => p.Name.Contains(query) || p.Category.Contains(query) || p.Location.Contains(query) || p.Description.Contains(query))
                .OrderByDescending(p => p.Rating)
                .Take(take)
                .Select(p => new SearchItem(p.Id, "program", p.Name, p.Location, p.ImageUrl, p.Rating))
                .ToListAsync();

            var guides = await context.Guides
                .AsNoTracking()
                .Where(g => g.FullName.Contains(query) || g.Description.Contains(query) || g.Languages.Contains(query) || g.Specialization.Contains(query))
                .OrderByDescending(g => g.Rating)
                .Take(take)
                .Select(g => new SearchItem(g.Id, "guide", g.FullName, g.Nationality, g.ImageUrl, g.Rating))
                .ToListAsync();

            var trips = await context.Trips
                .AsNoTracking()
                .Where(t => t.Title.Contains(query) || (t.Notes ?? string.Empty).Contains(query))
                .OrderByDescending(t => t.CreatedAt)
                .Take(take)
                .Select(t => new SearchItem(t.Id, "trip", t.Title, string.Empty, string.Empty, 0m))
                .ToListAsync();

            return Ok(new SearchResponse(places, services, hotels, transports, programs, guides, trips));
        }
    }

    public record SearchResponse(
        IReadOnlyCollection<SearchItem> Places,
        IReadOnlyCollection<SearchItem> Services,
        IReadOnlyCollection<SearchItem> Hotels,
        IReadOnlyCollection<SearchItem> Transports,
        IReadOnlyCollection<SearchItem> Programs,
        IReadOnlyCollection<SearchItem> Guides,
        IReadOnlyCollection<SearchItem> Trips
    );

    public record SearchItem(Guid Id, string Type, string Name, string LocationName, string ImageUrl, decimal Rating);
}
