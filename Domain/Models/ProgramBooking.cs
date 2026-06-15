namespace Domain.Models
{
    public class ProgramBooking
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public Guid ProgramId { get; set; }
        public Program? Program { get; set; }
        public int NumberOfParticipants { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "pending"; // pending, confirmed, declined, completed, cancelled
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
