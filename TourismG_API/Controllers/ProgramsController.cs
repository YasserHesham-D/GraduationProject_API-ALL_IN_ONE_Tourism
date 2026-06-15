using Application.Dtos.Programs;
using Application.Services.ProgramServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;
        private readonly ILogger<ProgramsController> _logger;

        public ProgramsController(IProgramService programService, ILogger<ProgramsController> logger)
        {
            _programService = programService;
            _logger = logger;
        }

        /// <summary>
        /// Get all programs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrograms()
        {
            try
            {
                var programs = await _programService.GetAllProgramsAsync();
                return Ok(new { success = true, data = programs });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting programs: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get program by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgramById(Guid id)
        {
            try
            {
                var program = await _programService.GetProgramByIdAsync(id);
                if (program == null)
                    return NotFound(new { success = false, message = "Program not found" });

                return Ok(new { success = true, data = program });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting program: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Book a program
        /// </summary>
        [HttpPost("{id}/book")]
        [Authorize]
        public async Task<IActionResult> BookProgram(Guid id, [FromBody] BookProgramRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var booking = await _programService.BookProgramAsync(id, request, userId);
                return Ok(new { success = true, data = booking, message = "Program booked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking program: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
