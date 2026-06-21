namespace Application.Dtos.Common
{
    public record ChatMessageDto(string SenderId, string? RecipientId, string Text, DateTime SentAt, string? ConversationKey = null);
    public record SendMessageRequest(string Text);
}