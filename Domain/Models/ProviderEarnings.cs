namespace Domain.Models
{
    public class ProviderEarnings
    {
        public Guid Id { get; set; }
        public string ProviderId { get; set; } = string.Empty;
        public User? Provider { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public int CompletedBookings { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
