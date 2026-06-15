namespace Application.Dtos.Programs
{
    public class CreateProgramRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int MaxParticipants { get; set; }
        public string IncludedServices { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
    }

    public class ProgramResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int MaxParticipants { get; set; }
        public int AvailableSpots { get; set; }
        public string IncludedServices { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class BookProgramRequest
    {
        public int NumberOfParticipants { get; set; }
    }

    public class ProgramBookingResponse
    {
        public Guid Id { get; set; }
        public Guid ProgramId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public int NumberOfParticipants { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
