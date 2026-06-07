namespace Domain.Models
{
    public class ServiceOffering
    {
        public Guid Id { get; set; }
        public string ProviderId { get; set; } = string.Empty;
        public User? Provider { get; set; }
        public Guid? PlaceId { get; set; }
        public Place? Place { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Duration { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int BookingCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
