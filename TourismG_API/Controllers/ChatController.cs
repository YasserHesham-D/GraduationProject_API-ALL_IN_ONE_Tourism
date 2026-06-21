using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(AppDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // GET: api/chat/history/{userA}/{userB}
        [HttpGet("history/{userA}/{userB}")]
        public async Task<IActionResult> GetChatHistory(string userA, string userB, [FromQuery] string? conversationKey = null)
        {
            // If userB is a guide id and no conversationKey was provided, treat userB as the conversation key and route to provider
            var guide = await _db.Guides.FirstOrDefaultAsync(g => g.Id.ToString() == userB);

            IQueryable<Domain.Models.ChatMessage> query;

            if (guide != null && string.IsNullOrEmpty(conversationKey))
            {
                conversationKey = guide.Id.ToString();
                var providerId = guide.ProviderId;

                query = _db.ChatMessages
                    .Where(m => m.ConversationKey == conversationKey && (
                        (m.SenderId == userA && m.RecipientId == providerId) || (m.SenderId == providerId && m.RecipientId == userA)
                    ));
            }
            else if (!string.IsNullOrEmpty(conversationKey))
            {
                // ConversationKey explicitly provided: restrict to that key between the two participants
                query = _db.ChatMessages
                    .Where(m => m.ConversationKey == conversationKey && (
                        (m.SenderId == userA && m.RecipientId == userB) || (m.SenderId == userB && m.RecipientId == userA)
                    ));
            }
            else
            {
                // Direct one-to-one message history between two user ids (no conversation key)
                query = _db.ChatMessages
                    .Where(m => (m.SenderId == userA && m.RecipientId == userB) || (m.SenderId == userB && m.RecipientId == userA));
            }

            var msgs = await query
                .OrderBy(m => m.SentAt)
                .Select(m => new Application.Dtos.Common.ChatMessageDto(m.SenderId, m.RecipientId, m.Text, m.SentAt, m.ConversationKey))
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
                // If ConversationKey is present, use it as the conversation discriminator so same pair can have multiple threads
                .Select(m => new { Other = m.ConversationKey != null ? m.ConversationKey : (m.SenderId == me ? m.RecipientId : m.SenderId), m.Text, m.SentAt })
                .Where(x => !string.IsNullOrEmpty(x.Other))
                .GroupBy(x => x.Other)
                .Select(g => new { OtherUserId = g.Key, LastMessage = g.OrderByDescending(x => x.SentAt).Select(x => x.Text).FirstOrDefault(), LastAt = g.Max(x => x.SentAt) })
                .OrderByDescending(x => x.LastAt)
                .ToListAsync();

            return Ok(conv);
        }
    }
}
