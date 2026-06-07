using System.Security.Claims;
using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController(UserManager<User> userManager, AppDbContext context) : ControllerBase
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
                user.Address,
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

            user.Address = request.Address;
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return NoContent();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing user id claim.");
        }
    }

    public record ProfileResponse(string Id, string FullName, string Email, string? Address, IReadOnlyCollection<string> Roles, ProfileStats Stats);
    public record ProfileStats(int Visited, int Trips, int Saved);
    public record UpdateProfileRequest(string? FullName, string? Address);
}
