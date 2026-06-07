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

            return Ok(new SearchResponse(places, services));
        }
    }

    public record SearchResponse(IReadOnlyCollection<SearchItem> Places, IReadOnlyCollection<SearchItem> Services);
    public record SearchItem(Guid Id, string Type, string Name, string LocationName, string ImageUrl, decimal Rating);
}
