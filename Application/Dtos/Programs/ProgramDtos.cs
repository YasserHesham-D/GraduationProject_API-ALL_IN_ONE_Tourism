namespace Application.Dtos.Programs
{
    public class CreateProgramRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int MaxParticipants { get; set; }
        public string? IncludedServices { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class ProgramResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int MaxParticipants { get; set; }
        public int AvailableSpots { get; set; }
        public string? IncludedServices { get; set; }
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
        public string? ProgramName { get; set; }
        public int NumberOfParticipants { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
