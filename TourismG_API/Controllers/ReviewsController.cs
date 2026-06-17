using System.Security.Claims;
using Application.Dtos.Common;
using Presentation.Services;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(AppDbContext context, IFileUploadService fileUploadService) : ControllerBase
    {
        [HttpGet("places/{placeId:guid}")]
        public async Task<IActionResult> GetPlaceReviews(Guid placeId)
        {
            if (!await context.Places.AnyAsync(p => p.Id == placeId))
            {
                return NotFound("Place not found.");
            }

            var reviews = await context.PlaceReviews
                .AsNoTracking()
                .Where(r => r.PlaceId == placeId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse(
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.User!.UserName ?? "Explorer",
                    r.CreatedAt))
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost("places/{placeId:guid}")]
        [Authorize]
        public async Task<IActionResult> AddOrUpdatePlaceReview(Guid placeId, [FromBody] CreateReviewRequest request)
        {
            if (!await context.Places.AnyAsync(p => p.Id == placeId))
            {
                return NotFound("Place not found.");
            }

            if (request.Rating is < 1 or > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var userId = GetUserId();
            var review = await context.PlaceReviews.FirstOrDefaultAsync(r => r.PlaceId == placeId && r.UserId == userId);
            if (review is null)
            {
                review = new PlaceReview
                {
                    PlaceId = placeId,
                    UserId = userId,
                    Rating = request.Rating,
                    Comment = request.Comment
                };
                await context.PlaceReviews.AddAsync(review);
            }
            else
            {
                review.Rating = request.Rating;
                review.Comment = request.Comment;
                review.CreatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            await RecalculatePlaceRatingAsync(placeId);
            await context.SaveChangesAsync();
            return Ok(new { review.Id });
        }

        [HttpGet("trips/{tripId:guid}")]
        public async Task<IActionResult> GetTripReviews(Guid tripId)
        {
            if (!await context.Trips.AnyAsync(t => t.Id == tripId))
            {
                return NotFound("Trip not found.");
            }

            var reviews = await context.TripReviews
                .AsNoTracking()
                .Where(r => r.TripId == tripId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse(
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.User!.UserName ?? "Explorer",
                    r.CreatedAt))
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost("trips/{tripId:guid}")]
        [Authorize]
        public async Task<IActionResult> AddOrUpdateTripReview(Guid tripId, [FromBody] CreateReviewRequest request)
        {
            if (!await context.Trips.AnyAsync(t => t.Id == tripId))
            {
                return NotFound("Trip not found.");
            }

            if (request.Rating is < 1 or > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var userId = GetUserId();
            var review = await context.TripReviews.FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == userId);
            if (review is null)
            {
                review = new TripReview
                {
                    TripId = tripId,
                    UserId = userId,
                    Rating = request.Rating,
                    Comment = request.Comment
                };
                await context.TripReviews.AddAsync(review);
            }
            else
            {
                review.Rating = request.Rating;
                review.Comment = request.Comment;
                review.CreatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return Ok(new { review.Id });
        }

        private async Task RecalculatePlaceRatingAsync(Guid placeId)
        {
            var place = await context.Places.FirstAsync(p => p.Id == placeId);
            var ratings = await context.PlaceReviews.Where(r => r.PlaceId == placeId).Select(r => r.Rating).ToListAsync();
            if (ratings.Count == 0)
            {
                return;
            }

            place.Rating = Math.Round((decimal)ratings.Average(), 2);
            place.ReviewCount = ratings.Count;
        }

        [HttpPost("{reviewId:guid}/upload-photo")]
        [Authorize]
        public async Task<IActionResult> UploadReviewPhoto(Guid reviewId, [FromForm] UploadPhotoRequest request)
        {
            var userId = GetUserId();
            var review = await context.PlaceReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review is null)
            {
                return Unauthorized("Not your review");
            }

            if (request.Photo is null || request.Photo.Length == 0)
            {
                return BadRequest("No file provided");
            }

            try
            {
                if (!fileUploadService.ValidateFile(request.Photo))
                {
                    return BadRequest("Invalid file. Allowed formats: JPG, PNG, WEBP, GIF, BMP. Max size: 5 MB");
                }

                var photoUrl = await fileUploadService.UploadFileAsync(request.Photo, "uploads");

                // Delete old photo if it exists
                if (!string.IsNullOrEmpty(review.PhotoUrl))
                {
                    fileUploadService.DeleteFile(review.PhotoUrl);
                }

                review.PhotoUrl = photoUrl;
                await context.SaveChangesAsync();

                return Ok(new UploadPhotoResponse(true, photoUrl));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new UploadPhotoResponse(false, null, $"Upload failed: {ex.Message}"));
            }
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record CreateReviewRequest(int Rating, string Comment);
    public record ReviewResponse(Guid Id, int Rating, string Comment, string Username, DateTime CreatedAt);
}
