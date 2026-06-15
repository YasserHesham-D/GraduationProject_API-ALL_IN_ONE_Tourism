namespace Domain.Models
{
    public class Transport
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Bus, Car, Taxi, etc.
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string DepartureLocation { get; set; } = string.Empty;
        public string ArrivalLocation { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalCapacity { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? ProviderId { get; set; }
        public User? Provider { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TransportBooking> Bookings { get; set; } = new List<TransportBooking>();
    }
}
