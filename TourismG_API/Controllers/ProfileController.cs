using System.Security.Claims;
using Application.Dtos.Common;
using Presentation.Services;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController(UserManager<User> userManager, AppDbContext context, IFileUploadService fileUploadService, IConfiguration configuration) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(user);
            var stats = new ProfileStats(
                await context.VisitedPlaces.CountAsync(v => v.UserId == userId),
                await context.Trips.CountAsync(t => t.UserId == userId),
                await context.SavedPlaces.CountAsync(s => s.UserId == userId));

            return Ok(new ProfileResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                roles.ToArray(),
                stats));
        }

        [HttpPut("me")]
        
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
        {
            var user = await userManager.FindByIdAsync(GetUserId());
            if (user is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.UserName = request.FullName;
            }

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return NoContent();
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadPhotoRequest request)
        {
            var userId = GetUserId();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound("User not found");
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

                // Build absolute URL for clients
                var baseUrl = configuration["PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var absoluteUrl = $"{baseUrl}{photoUrl}";

                // Delete old avatar if it exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    fileUploadService.DeleteFile(user.ProfileImageUrl);
                }

                user.ProfileImageUrl = photoUrl;
                var result = await userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors.Select(e => e.Description));
                }

                return Ok(new UploadPhotoResponse(true, absoluteUrl));
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

    public record ProfileResponse(string Id, string FullName, string Email, IReadOnlyCollection<string> Roles, ProfileStats Stats);
    public record ProfileStats(int Visited, int Trips, int Saved);
    public record UpdateProfileRequest(string? FullName, string? Address);
}
