using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Guide
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty; // Comma-separated
        public string Specialization { get; set; } = string.Empty; // History, Nature, etc.
        [MaxLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? ProviderId { get; set; }
        public User? Provider { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GuideBooking> Bookings { get; set; } = new List<GuideBooking>();
    }
}
