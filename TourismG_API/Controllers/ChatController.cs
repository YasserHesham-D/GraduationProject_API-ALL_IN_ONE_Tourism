using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/chat/history/{userA}/{userB}
        [HttpGet("history/{userA}/{userB}")]
        public async Task<IActionResult> GetChatHistory(string userA, string userB)
        {
            var msgs = await _db.ChatMessages
                .Where(m => (m.SenderId == userA && m.RecipientId == userB) || (m.SenderId == userB && m.RecipientId == userA))
                .OrderBy(m => m.SentAt)
                .Select(m => new {  m.Text, m.SentAt })
                .ToListAsync();

            return Ok(msgs);
        }

        // GET: api/chat/myconversations
        [HttpGet("myconversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var me = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(me))
            {
                // Helpful debugging response for dev: return claims so you can see which claim holds the user id
                var claims = User.Claims.Select(c => new { c.Type, c.Value });
                return BadRequest(new { message = "Missing NameIdentifier claim. Check JWT mapping.", claims });
            }

            var conv = await _db.ChatMessages
                .Where(m => m.SenderId == me || m.RecipientId == me)
            .Select(m => new { OtherUser = m.SenderId == me ? m.RecipientId : m.SenderId, m.Text, m.SentAt })
            .Where(x => !string.IsNullOrEmpty(x.OtherUser))
            .GroupBy(x => x.OtherUser)
            .Select(g => new { OtherUserId = g.Key, LastMessage = g.OrderByDescending(x => x.SentAt).Select(x => x.Text).FirstOrDefault(), LastAt = g.Max(x => x.SentAt) })
            .OrderByDescending(x => x.LastAt)
            .ToListAsync();

            return Ok(conv);
        }
    }
}
