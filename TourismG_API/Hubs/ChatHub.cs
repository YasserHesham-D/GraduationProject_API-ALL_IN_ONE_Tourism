using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Application.Dtos.Common;
using Infrastructure.DbContext;
using Domain.Models;

namespace Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const string UserIdClaim = ClaimTypes.NameIdentifier;
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public override Task OnConnectedAsync()
        {
            // Optionally: track presence (user -> connectionId mapping) here
            return base.OnConnectedAsync();
        }

        // Send direct message to a specific user and persist it
        public async Task SendMessageToUser(string targetUserId, SendMessageRequest message)
        {
            var senderId = Context.User?.FindFirst(UserIdClaim)?.Value ?? string.Empty;

            var chatEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = targetUserId,
                Text = message.Text ?? string.Empty,
                SentAt = DateTime.UtcNow
            };

            _db.ChatMessages.Add(chatEntity);
            await _db.SaveChangesAsync();

            var dto = new ChatMessageDto(chatEntity.SenderId, chatEntity.RecipientId, chatEntity.Text, chatEntity.SentAt);

            // Send to recipient and echo to the sender
            await Clients.User(targetUserId).SendAsync("ReceiveMessage", dto);
            await Clients.Caller.SendAsync("ReceiveMessage", dto);
        }
    }
}
