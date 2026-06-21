using System;

namespace Domain.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string? RecipientId { get; set; }
        // ConversationKey distinguishes separate one-to-one threads between the same two users
        // e.g. when provider and customer have multiple guides, set ConversationKey = guideId to keep threads separate
        public string? ConversationKey { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}