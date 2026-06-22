using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Program
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [MaxLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Adventure, Cultural, Beach, etc.
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public decimal Price { get; set; }
        public int Duration { get; set; } // in hours
        public int MaxParticipants { get; set; }
        public int AvailableSpots { get; set; }
        public string IncludedServices { get; set; } = string.Empty; // JSON or comma-separated
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? ProviderId { get; set; }
        public User? Provider { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProgramBooking> Bookings { get; set; } = new List<ProgramBooking>();
    }
}
