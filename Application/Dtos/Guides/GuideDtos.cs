namespace Application.Dtos.Guides
{
    public class CreateGuideRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
    }

    public class GuideResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
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
        public string SpecialRequests { get; set; } = string.Empty;
    }

    public class GuideBookingResponse
    {
        public Guid Id { get; set; }
        public Guid GuideId { get; set; }
        public string GuideName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPeople { get; set; }
        public int NumberOfDays { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
