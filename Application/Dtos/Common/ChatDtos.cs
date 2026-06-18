namespace Application.Dtos.Common
{
    public record ChatMessageDto(string SenderId, string? RecipientId, string Text, DateTime SentAt);
    public record SendMessageRequest(string Text);
}