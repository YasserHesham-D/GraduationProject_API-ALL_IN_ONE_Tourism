namespace Domain.Models
{
    public class TripActivity
    {
        public Guid Id { get; set; }
        public Guid TripDayId { get; set; }
        public TripDay? TripDay { get; set; }
        public Guid? PlaceId { get; set; }
        public Place? Place { get; set; }
        public Guid? ServiceOfferingId { get; set; }
        public ServiceOffering? ServiceOffering { get; set; }
        public string Title { get; set; } = string.Empty;
        public TimeOnly? ScheduledAt { get; set; }
        public string? Notes { get; set; }
    }
}
