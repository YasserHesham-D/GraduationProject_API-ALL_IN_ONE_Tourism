using Application.Dtos.Transport;
using Application.Services.TransportServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransportController : ControllerBase
    {
        private readonly ITransportService _transportService;
        private readonly ILogger<TransportController> _logger;

        public TransportController(ITransportService transportService, ILogger<TransportController> logger)
        {
            _transportService = transportService;
            _logger = logger;
        }

        /// <summary>
        /// Get all transport options
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransports()
        {
            try
            {
                var transports = await _transportService.GetAllTransportsAsync();
                return Ok(new { success = true, data = transports });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transports: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get transport by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransportById(Guid id)
        {
            try
            {
                var transport = await _transportService.GetTransportByIdAsync(id);
                if (transport == null)
                    return NotFound(new { success = false, message = "Transport not found" });

                return Ok(new { success = true, data = transport });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting transport: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book transport
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookTransport(Guid id, [FromBody] BookTransportRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _transportService.BookTransportAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Transport booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking transport: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
