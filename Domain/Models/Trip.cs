namespace Domain.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TripDay> Days { get; set; } = new List<TripDay>();
        public ICollection<TripReview> Reviews { get; set; } = new List<TripReview>();
    }
}
