namespace Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public Guid ServiceOfferingId { get; set; }
        public ServiceOffering? ServiceOffering { get; set; }
        public DateTime BookingDate { get; set; }
        public int Guests { get; set; } = 1;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
