using Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPlaces(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] bool? recommended,
            [FromQuery] bool? popular,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = context.Places.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.City.Contains(search) ||
                    p.Category.Contains(search) ||
                    p.LocationName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Category == category);
            }

            if (recommended.HasValue)
            {
                query = query.Where(p => p.IsRecommended == recommended.Value);
            }

            if (popular.HasValue)
            {
                query = query.Where(p => p.IsPopular == popular.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.IsRecommended)
                .ThenByDescending(p => p.Rating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PlaceListItem(
                    p.Id,
                    p.Name,
                    p.Category,
                    p.LocationName,
                    p.City,
                    p.Country,
                    p.ImageUrl,
                    p.Rating,
                    p.ReviewCount,
                    p.PriceFrom,
                    p.IsRecommended,
                    p.IsPopular))
                .ToListAsync();

            return Ok(new PagedResponse<PlaceListItem>(items, page, pageSize, totalCount));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPlace(Guid id)
        {
            var placeEntity = await context.Places
                .AsNoTracking()
                .Include(p => p.Services)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (placeEntity is null)
            {
                return NotFound();
            }

            var nearby = await context.Places
                .AsNoTracking()
                .Where(p => p.Id != id && (p.City == placeEntity.City || p.Category == placeEntity.Category))
                .OrderByDescending(p => p.City == placeEntity.City)
                .ThenByDescending(p => p.Rating)
                .Take(5)
                .Select(p => new NearbyPlaceDto(p.Id, p.Name, p.Rating, p.LocationName, p.ImageUrl))
                .ToListAsync();

            var place = new PlaceDetails(
                    placeEntity.Id,
                    placeEntity.Name,
                    placeEntity.Category,
                    placeEntity.LocationName,
                    placeEntity.City,
                    placeEntity.Country,
                    placeEntity.Description,
                    placeEntity.ImageUrl,
                    placeEntity.OpeningHours,
                    placeEntity.Rating,
                    placeEntity.ReviewCount,
                    placeEntity.PriceFrom,
                    placeEntity.DistanceKm,
                    placeEntity.Latitude,
                    placeEntity.Longitude,
                    placeEntity.Services.Where(s => s.IsActive).Select(s => new ServiceSummary(
                        s.Id,
                        s.Title,
                        s.Category,
                        s.Price,
                        s.Currency,
                        s.Duration,
                        s.LocationName,
                        s.ImageUrl,
                        s.Rating,
                        s.BookingCount)).ToList(),
                    nearby,
                    placeEntity.Reviews.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new ReviewDto(
                        r.Id,
                        r.Rating,
                        r.Comment,
                        r.User?.UserName ?? "Explorer",
                        r.CreatedAt)).ToList());

            return Ok(place);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetPlacesByCategory(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return await GetPlaces(null, category, null, null, page, pageSize);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetPlaceSummaries()
        {
            var places = await context.Places
                .AsNoTracking()
                .OrderByDescending(p => p.Rating)
                .Select(p => new PlaceSummary(
                    p.Id,
                    p.Name,
                    p.LocationName,
                    p.ImageUrl,
                    p.Rating,
                    p.PriceFrom))
                .ToListAsync();

            return Ok(places);
        }

        [HttpGet("{id:guid}/nearby")]
        public async Task<IActionResult> GetNearbyPlaces(Guid id, [FromQuery] int take = 5)
        {
            take = Math.Clamp(take, 1, 20);
            var source = await context.Places.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (source is null)
            {
                return NotFound();
            }

            var nearby = await context.Places
                .AsNoTracking()
                .Where(p => p.Id != id && (p.City == source.City || p.Category == source.Category))
                .OrderByDescending(p => p.City == source.City)
                .ThenByDescending(p => p.Rating)
                .Take(take)
                .Select(p => new PlaceListItem(
                    p.Id,
                    p.Name,
                    p.Category,
                    p.LocationName,
                    p.City,
                    p.Country,
                    p.ImageUrl,
                    p.Rating,
                    p.ReviewCount,
                    p.PriceFrom,
                    p.IsRecommended,
                    p.IsPopular))
                .ToListAsync();

            return Ok(nearby);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await context.Places
                .AsNoTracking()
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(new[] { "All" }.Concat(categories));
        }
    }

    public record PagedResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);

    public record PlaceListItem(
        Guid Id,
        string Name,
        string Category,
        string LocationName,
        string City,
        string Country,
        string ImageUrl,
        decimal Rating,
        int ReviewCount,
        decimal? PriceFrom,
        bool IsRecommended,
        bool IsPopular);

    public record PlaceDetails(
        Guid Id,
        string Name,
        string Category,
        string LocationName,
        string City,
        string Country,
        string Description,
        string ImageUrl,
        string OpeningHours,
        decimal Rating,
        int ReviewCount,
        decimal? PriceFrom,
        decimal? DistanceKm,
        decimal? Latitude,
        decimal? Longitude,
        IReadOnlyCollection<ServiceSummary> Services,
        IReadOnlyCollection<NearbyPlaceDto> NearbyPlaces,
        IReadOnlyCollection<ReviewDto> Reviews);

    public record PlaceSummary(Guid Id, string Name, string LocationName, string ImageUrl, decimal Rating, decimal? PriceFrom);
    public record NearbyPlaceDto(Guid Id, string Name, decimal Rating, string LocationName, string ImageUrl);
    public record ReviewDto(Guid Id, int Rating, string Comment, string Username, DateTime CreatedAt);

    public record ServiceSummary(
        Guid Id,
        string Title,
        string Category,
        decimal Price,
        string Currency,
        string Duration,
        string LocationName,
        string ImageUrl,
        decimal Rating,
        int BookingCount);
}
