namespace Application.Dtos.Guides
{
    public class CreateGuideRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Nationality { get; set; }
        public string? Languages { get; set; }
        public string? Specialization { get; set; }
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public decimal PricePerDay { get; set; }
    }

    public class GuideResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Nationality { get; set; }
        public string? Languages { get; set; }
        public string? Specialization { get; set; }
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class BookGuideRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPeople { get; set; }
        public string? SpecialRequests { get; set; }
    }

    public class GuideBookingResponse
    {
        public Guid Id { get; set; }
        public Guid GuideId { get; set; }
        public string? GuideName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
