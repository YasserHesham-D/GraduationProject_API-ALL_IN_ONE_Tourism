namespace Application.Dtos.Transport
{
    public class CreateTransportRequest
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public string? DepartureTime { get; set; }
        public string? ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int TotalCapacity { get; set; }
    }

    public class TransportResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DepartureLocation { get; set; }
        public string? ArrivalLocation { get; set; }
        public string? DepartureTime { get; set; }
        public string? ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalCapacity { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class BookTransportRequest
    {
        public int NumberOfSeats { get; set; }
    }

    public class TransportBookingResponse
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public string? TransportName { get; set; }
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
