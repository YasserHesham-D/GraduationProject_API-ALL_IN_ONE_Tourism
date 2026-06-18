using System;

namespace Domain.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string? RecipientId { get; set; }
        public string? GroupName { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}