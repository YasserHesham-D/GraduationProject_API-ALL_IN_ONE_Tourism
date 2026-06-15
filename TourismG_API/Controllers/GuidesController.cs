using Application.Dtos.Guides;
using Application.Services.GuideServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuidesController : ControllerBase
    {
        private readonly IGuideService _guideService;
        private readonly ILogger<GuidesController> _logger;

        public GuidesController(IGuideService guideService, ILogger<GuidesController> logger)
        {
            _guideService = guideService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available guides
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGuides()
        {
            try
            {
                var guides = await _guideService.GetAllGuidesAsync();
                return Ok(new { success = true, data = guides });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting guides: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get guide by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuideById(Guid id)
        {
            try
            {
                var guide = await _guideService.GetGuideByIdAsync(id);
                if (guide == null)
                    return NotFound(new { success = false, message = "Guide not found" });

                return Ok(new { success = true, data = guide });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting guide: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book a guide
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookGuide(Guid id, [FromBody] BookGuideRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _guideService.BookGuideAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Guide booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking guide: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
