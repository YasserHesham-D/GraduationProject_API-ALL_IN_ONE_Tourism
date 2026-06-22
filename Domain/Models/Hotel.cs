using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Hotel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public string Description { get; set; } = string.Empty;
        [MaxLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;
        public int StarRating { get; set; } // 1-5 stars
        public decimal PricePerNight { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int AvailableRooms { get; set; }
        public string Amenities { get; set; } = string.Empty; // JSON or comma-separated
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProviderId { get; set; }
        public User? Provider { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<HotelBooking> Bookings { get; set; } = new List<HotelBooking>();
    }
}
