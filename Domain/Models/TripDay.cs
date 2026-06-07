namespace Domain.Models
{
    public class TripDay
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public Trip? Trip { get; set; }
        public int DayNumber { get; set; }
        public DateOnly Date { get; set; }

        public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();
    }
}
