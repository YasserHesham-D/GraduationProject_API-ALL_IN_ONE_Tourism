using Application.Dtos.Hotels;
using Application.Services.HotelServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepo _hotelRepo;
        private readonly IHotelBookingRepo _bookingRepo;
        private readonly ILogger<HotelService> _logger;

        public HotelService(IHotelRepo hotelRepo, IHotelBookingRepo bookingRepo, ILogger<HotelService> logger)
        {
            _hotelRepo = hotelRepo;
            _bookingRepo = bookingRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<HotelResponse>> GetAllHotelsAsync()
        {
            var hotels = await _hotelRepo.GetAllAsync();
            return hotels.Select(MapToResponse);
        }

        public async Task<HotelResponse?> GetHotelByIdAsync(Guid id)
        {
            var hotel = await _hotelRepo.GetByIdAsync(id);
            return hotel != null ? MapToResponse(hotel) : null;
        }

        public async Task<HotelResponse> CreateHotelAsync(CreateHotelRequest request, string providerId)
        {
            var hotel = new Hotel
            {
                Name = request.Name,
                Location = request.Location,
                City = request.City,
                Country = request.Country,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                StarRating = request.StarRating,
                PricePerNight = request.PricePerNight,
                AvailableRooms = request.AvailableRooms,
                Amenities = request.Amenities,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                ProviderId = providerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdHotel = await _hotelRepo.AddAsync(hotel);
            await _hotelRepo.SaveChangesAsync();
            return MapToResponse(createdHotel);
        }

        public async Task<HotelBookingResponse> BookHotelAsync(Guid hotelId, BookHotelRequest request, string userId)
        {
            var hotel = await _hotelRepo.GetByIdAsync(hotelId);
            if (hotel == null)
                throw new Exception("Hotel not found");

            if (hotel.AvailableRooms < request.NumberOfRooms)
                throw new Exception("Not enough available rooms");

            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            var totalPrice = hotel.PricePerNight * request.NumberOfRooms * nights;

            var booking = new HotelBooking
            {
                UserId = userId,
                HotelId = hotelId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfRooms = request.NumberOfRooms,
                NumberOfGuests = request.NumberOfGuests,
                TotalPrice = totalPrice,
                Status = "pending",
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepo.AddAsync(booking);
            // Save booking first to ensure it's persisted
            await _bookingRepo.SaveChangesAsync();

            // Update hotel's available rooms
            hotel.AvailableRooms -= request.NumberOfRooms;
            await _hotelRepo.UpdateAsync(hotel);
            await _hotelRepo.SaveChangesAsync();

            return MapBookingToResponse(createdBooking, hotel.Name);
        }

        public async Task<IEnumerable<HotelBookingResponse>> GetUserHotelBookingsAsync(string userId)
        {
            var bookings = await _bookingRepo.GetUserBookingsAsync(userId);
            return bookings.Select(b => MapBookingToResponse(b, b.Hotel?.Name ?? "Unknown"));
        }

        public async Task<IEnumerable<HotelBookingResponse>> GetProviderHotelBookingsAsync(string providerId)
        {
            var bookings = await _bookingRepo.GetProviderBookingsAsync(providerId);
            return bookings.Select(b => MapBookingToResponse(b, b.Hotel?.Name ?? "Unknown"));
        }

        private HotelResponse MapToResponse(Hotel hotel)
        {
            return new HotelResponse
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Location = hotel.Location,
                City = hotel.City,
                Country = hotel.Country,
                Description = hotel.Description,
                ImageUrl = hotel.ImageUrl,
                StarRating = hotel.StarRating,
                PricePerNight = hotel.PricePerNight,
                Rating = hotel.Rating,
                ReviewCount = hotel.ReviewCount,
                AvailableRooms = hotel.AvailableRooms,
                Amenities = hotel.Amenities,
                ContactNumber = hotel.ContactNumber,
                Email = hotel.Email
            };
        }

        private HotelBookingResponse MapBookingToResponse(HotelBooking booking, string hotelName)
        {
            return new HotelBookingResponse
            {
                Id = booking.Id,
                HotelId = booking.HotelId,
                HotelName = hotelName,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfRooms = booking.NumberOfRooms,
                NumberOfGuests = booking.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }
    }
}
