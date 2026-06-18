using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                .Select(p => new PlaceListItemR(
                    p.Id,
                    p.Name,
                    p.City,
                    p.Country,
                    p.ImageUrl,
                    p.Rating,
                    p.IsRecommended,
                    p.IsPopular))
                .ToListAsync();

            return Ok(new PagedResponse<PlaceListItemR>(items, page, pageSize, totalCount));
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
                        s.StartDateTime,
                        s.EndDateTime,
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

        [HttpPost("[Action]")]
        public async Task<IActionResult> AddPlace([FromBody] CreatePlaceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { success = false, message = "Place name is required" });

            if (string.IsNullOrWhiteSpace(request.City))
                return BadRequest(new { success = false, message = "City is required" });

            if (string.IsNullOrWhiteSpace(request.LocationName))
                return BadRequest(new { success = false, message = "Location name is required" });

            var place = new Place
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Category = request.Category ?? string.Empty,
                LocationName = request.LocationName,
                City = request.City,
                Country = request.Country ?? "Egypt",
                Description = request.Description ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                OpeningHours = request.OpeningHours ?? string.Empty,
                Rating = request.Rating ?? 0m,
                ReviewCount = 0,
                PriceFrom = request.PriceFrom,
                DistanceKm = request.DistanceKm,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsRecommended = request.IsRecommended ?? false,
                IsPopular = request.IsPopular ?? false,
                CreatedAt = DateTime.UtcNow
            };

            await context.Places.AddAsync(place);
            await context.SaveChangesAsync();

            return Ok(new{message = "Place added successfully" });
        }


        // Place Management Endpoints
        //[HttpGet("places")]
        //public async Task<IActionResult> GetMyPlaces()
        //{
        //    var providerId = GetUserId();
        //    var places = await context.Places
        //        .AsNoTracking()
        //        .OrderByDescending(p => p.CreatedAt)
        //        .Select(p => new ProviderPlaceItem(
        //            p.Id,
        //            p.Name,
        //            p.Category,
        //            p.City,
        //            p.Country,
        //            p.LocationName,
        //            p.Description,
        //            p.ImageUrl,
        //            p.OpeningHours,
        //            p.Rating,
        //            p.ReviewCount,
        //            p.PriceFrom,
        //            p.DistanceKm,
        //            p.Latitude,
        //            p.Longitude,
        //            p.IsRecommended,
        //            p.IsPopular))
        //        .ToListAsync();

        //    return Ok(places);
        //}

        [HttpPut("places/{id:guid}")]
        public async Task<IActionResult> UpdatePlace(Guid id, [FromBody] CreatePlaceRequest request)
        {
            var place = await context.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place is null)
                return NotFound(new { success = false, message = "Place not found" });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { success = false, message = "Place name is required" });

            if (string.IsNullOrWhiteSpace(request.City))
                return BadRequest(new { success = false, message = "City is required" });

            place.Name = request.Name;
            place.Category = request.Category ?? place.Category;
            place.LocationName = request.LocationName ?? place.LocationName;
            place.City = request.City;
            place.Country = request.Country ?? place.Country;
            place.Description = request.Description ?? place.Description;
            place.ImageUrl = request.ImageUrl ?? place.ImageUrl;
            place.OpeningHours = request.OpeningHours ?? place.OpeningHours;
            place.PriceFrom = request.PriceFrom ?? place.PriceFrom;
            place.DistanceKm = request.DistanceKm ?? place.DistanceKm;
            place.Latitude = request.Latitude ?? place.Latitude;
            place.Longitude = request.Longitude ?? place.Longitude;
            place.IsRecommended = request.IsRecommended ?? place.IsRecommended;
            place.IsPopular = request.IsPopular ?? place.IsPopular;

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("places/{id:guid}")]
        public async Task<IActionResult> DeletePlace(Guid id)
        {
            var place = await context.Places.FirstOrDefaultAsync(p => p.Id == id);
            if (place is null)
                return NotFound(new { success = false, message = "Place not found" });

            context.Places.Remove(place);
            await context.SaveChangesAsync();
            return NoContent();
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
                    p.Rating
                    ))
                .ToListAsync();

            return Ok(places);
        }

        [HttpGet("[Action]")]
        public async Task<IActionResult> GetRecommendedPlaces()
        {

            var Recomended = await context.Places
                .AsNoTracking()
                .Where(p => p.IsRecommended).Where(x => x.IsPopular).OrderBy(x => x.Rating)
                .Take(5)
                .Select(p => new PlaceListItemR(
                    p.Id,
                    p.Name,
                    p.City,
                    p.Country,
                    p.ImageUrl,
                    p.Rating,
                    p.IsRecommended,
                    p.IsPopular))
                .ToListAsync();

            return Ok(Recomended);
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
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record PagedResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
    public record PlaceListItemR(
    Guid Id,
    string Name,
    string City,
    string Country,
    string ImageUrl,
    decimal Rating,
    bool IsRecommended,
    bool IsPopular);
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

    public record PlaceSummary(Guid Id, string Name, string LocationName, string ImageUrl, decimal Rating);
    public record NearbyPlaceDto(Guid Id, string Name, decimal Rating, string LocationName, string ImageUrl);
    public record ReviewDto(Guid Id, int Rating, string Comment, string Username, DateTime CreatedAt);
    public record CreatePlaceRequest(string Name, string? Category, string LocationName, string City, string? Country, string? Description, string? ImageUrl, string? OpeningHours, decimal? Rating, decimal? PriceFrom, decimal? DistanceKm, decimal? Latitude, decimal? Longitude, bool? IsRecommended, bool? IsPopular);

    public record ServiceSummary(
        Guid Id,
        string Title,
        string Category,
        decimal Price,
        string Currency,
        DateTime StartDateTime,
        DateTime EndDateTime,
        string LocationName,
        string ImageUrl,
        decimal Rating,
        int BookingCount);
}
