namespace Domain.Models
{
    public class PlaceReview
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
