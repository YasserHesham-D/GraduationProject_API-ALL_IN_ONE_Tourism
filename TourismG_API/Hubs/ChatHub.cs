using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Application.Dtos.Common;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const string UserIdClaim = ClaimTypes.NameIdentifier;
        private readonly AppDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        // userId -> set of connectionIds
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new(StringComparer.OrdinalIgnoreCase);

        public ChatHub(AppDbContext db, ILogger<ChatHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(UserIdClaim)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var conns = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
                lock (conns)
                {
                    conns.Add(Context.ConnectionId);
                }
                _logger.LogDebug("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(UserIdClaim)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                if (_userConnections.TryGetValue(userId, out var conns))
                {
                    lock (conns)
                    {
                        conns.Remove(Context.ConnectionId);
                        if (conns.Count == 0)
                        {
                            _userConnections.TryRemove(userId, out _);
                        }
                    }
                }
                _logger.LogDebug("User {UserId} disconnected connection {ConnectionId}", userId, Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        // Send direct message to a specific user and persist it
        public async Task SendMessageToUser(string targetUserId, SendMessageRequest message, string? conversationKey = null)
        {
            var senderId = Context.User?.FindFirst(UserIdClaim)?.Value ?? string.Empty;
            // Resolve guide -> provider mapping: if the targetUserId is a Guide.Id and no explicit conversationKey supplied,
            // treat targetUserId as guide and route recipient to provider and set conversationKey = guide.Id.
            var guide = await _db.Guides.FirstOrDefaultAsync(g => g.Id.ToString() == targetUserId);

            string? recipientId;

            if (guide != null && string.IsNullOrEmpty(conversationKey))
            {
                // caller passed a guide id as target. Use it as conversation key and route to provider.
                conversationKey = guide.Id.ToString();
                if (senderId != guide.ProviderId)
                {
                    // customer -> guide: send to provider
                    recipientId = guide.ProviderId;
                }
                else
                {
                    // Provider sent targeting a guide id without specifying customer: try to infer the last customer that participated in this conversationKey.
                    var lastOther = await _db.ChatMessages
                        .Where(m => m.ConversationKey == conversationKey && (m.SenderId != senderId || (m.RecipientId != null && m.RecipientId != senderId)))
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefaultAsync();

                    if (lastOther != null)
                    {
                        recipientId = lastOther.SenderId != senderId ? lastOther.SenderId : lastOther.RecipientId;
                    }
                    else
                    {
                        // No prior participant found — require explicit customer id to avoid misrouting
                        throw new HubException("No recent participant found for this guide conversation. Provider must specify the customer user id as targetUserId when sending to a guide conversation.");
                    }
                }
            }
            else
            {
                // targetUserId is a user id (customer or provider)
                recipientId = targetUserId;
            }

            var chatEntity = new ChatMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                ConversationKey = string.IsNullOrEmpty(conversationKey) ? null : conversationKey,
                Text = message.Text ?? string.Empty,
                SentAt = DateTime.UtcNow
            };

            _db.ChatMessages.Add(chatEntity);
            await _db.SaveChangesAsync();

            var dto = new ChatMessageDto(chatEntity.SenderId, chatEntity.RecipientId, chatEntity.Text, chatEntity.SentAt, chatEntity.ConversationKey);

            // Send to the appropriate real-time target: deliver to recipient user id and echo to caller.


                if (!string.IsNullOrEmpty(chatEntity.RecipientId))
                {
                    // Try targeted delivery to all the recipient's active connections
                    if (_userConnections.TryGetValue(chatEntity.RecipientId, out var recipientConns) && recipientConns.Count > 0)
                    {
                        var clients = recipientConns.ToList();
                        await Clients.Clients(clients).SendAsync("ReceiveMessage", dto);
                        _logger.LogDebug("Delivered message {MessageId} to recipient {RecipientId} connections {Connections}", chatEntity.Id, chatEntity.RecipientId, string.Join(',', clients));
                    }
                    else
                    {
                        // Fallback to Clients.User (in case user id provider is configured differently)
                        await Clients.User(chatEntity.RecipientId).SendAsync("ReceiveMessage", dto);
                        _logger.LogDebug("Recipient {RecipientId} had no tracked connections; used Clients.User fallback", chatEntity.RecipientId);
                    }
                }

                // Echo to caller
                await Clients.Caller.SendAsync("ReceiveMessage", dto);

        }

        // Allow clients to join a guide-specific conversation group (so they receive guide-scoped messages)

    }
}
