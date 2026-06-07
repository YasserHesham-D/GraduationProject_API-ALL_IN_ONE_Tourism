namespace Domain.Models
{
    public class TripReview
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public Trip? Trip { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
