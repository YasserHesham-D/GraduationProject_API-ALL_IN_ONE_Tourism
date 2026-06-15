namespace Application.Dtos.Transport
{
    public class CreateTransportRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string DepartureLocation { get; set; } = string.Empty;
        public string ArrivalLocation { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalCapacity { get; set; }
    }

    public class TransportResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string DepartureLocation { get; set; } = string.Empty;
        public string ArrivalLocation { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
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
        public string TransportName { get; set; } = string.Empty;
        public int NumberOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
