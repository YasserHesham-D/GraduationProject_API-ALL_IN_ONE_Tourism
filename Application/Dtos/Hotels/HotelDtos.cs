namespace Application.Dtos.Hotels
{
    public class CreateHotelRequest
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int StarRating { get; set; }
        public decimal PricePerNight { get; set; }
        public int AvailableRooms { get; set; }
        public string? Amenities { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
    }

    public class HotelResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int StarRating { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int AvailableRooms { get; set; }
        public string? Amenities { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
    }

    public class BookHotelRequest
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfGuests { get; set; }
        public string? SpecialRequests { get; set; }
    }

    public class HotelBookingResponse
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public string? HotelName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
