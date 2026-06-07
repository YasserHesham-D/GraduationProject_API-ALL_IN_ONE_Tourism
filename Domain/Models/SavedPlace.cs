namespace Domain.Models
{
    public class SavedPlace
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
