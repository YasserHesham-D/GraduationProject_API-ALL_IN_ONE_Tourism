using Application.Dtos.ProviderManagement;
using Application.Services.ProviderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProviderService _providerService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IProviderService providerService, ILogger<AdminController> logger)
        {
            _providerService = providerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all pending provider requests
        /// </summary>
        [HttpGet("provider-requests")]
        public async Task<IActionResult> GetProviderRequests()
        {
            try
            {
                var requests = await _providerService.GetPendingProviderRequestsAsync();
                return Ok(new { success = true, data = requests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting provider requests: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get provider request by ID
        /// </summary>
        [HttpGet("provider-requests/{id}")]
        public async Task<IActionResult> GetProviderRequestById(Guid id)
        {
            try
            {
                // Note: This would need an additional method in the service to get by ID
                // For now, we'll assume the request is available through the pending requests
                var requests = await _providerService.GetPendingProviderRequestsAsync();
                var request = requests.FirstOrDefault(r => r.Id == id);

                if (request == null)
                    return NotFound(new { success = false, message = "Provider request not found" });

                return Ok(new { success = true, data = request });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting provider request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Approve a provider request
        /// </summary>
        [HttpPut("provider-requests/{id}/approve")]
        public async Task<IActionResult> ApproveProviderRequest(Guid id)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { success = false, message = "Admin not authenticated" });

                var response = await _providerService.ApproveProviderRequestAsync(id, adminId);
                if (response == null)
                    return NotFound(new { success = false, message = "Provider request not found" });

                return Ok(new { success = true, data = response, message = "Provider request approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving provider request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reject a provider request
        /// </summary>
        [HttpPut("provider-requests/{id}/reject")]
        public async Task<IActionResult> RejectProviderRequest(Guid id, [FromBody] RejectProviderRequestDto request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { success = false, message = "Admin not authenticated" });

                var response = await _providerService.RejectProviderRequestAsync(id, request.RejectionReason, adminId);
                if (response == null)
                    return NotFound(new { success = false, message = "Provider request not found" });

                return Ok(new { success = true, data = response, message = "Provider request rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting provider request: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
