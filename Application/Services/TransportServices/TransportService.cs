using Application.Dtos.Transport;
using Application.Services.TransportServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class TransportService : ITransportService
    {
        private readonly ITransportRepo _transportRepo;
        private readonly ITransportBookingRepo _bookingRepo;
        private readonly ILogger<TransportService> _logger;

        public TransportService(ITransportRepo transportRepo, ITransportBookingRepo bookingRepo, ILogger<TransportService> logger)
        {
            _transportRepo = transportRepo;
            _bookingRepo = bookingRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<TransportResponse>> GetAllTransportsAsync()
        {
            var transports = await _transportRepo.GetAllAsync();
            return transports.Select(MapToResponse);
        }

        public async Task<TransportResponse?> GetTransportByIdAsync(Guid id)
        {
            var transport = await _transportRepo.GetByIdAsync(id);
            return transport != null ? MapToResponse(transport) : null;
        }

        public async Task<TransportResponse> CreateTransportAsync(CreateTransportRequest request, string providerId)
        {
            var transport = new Transport
            {
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DepartureLocation = request.DepartureLocation,
                ArrivalLocation = request.ArrivalLocation,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                Price = request.Price,
                TotalCapacity = request.TotalCapacity,
                AvailableSeats = request.TotalCapacity,
                ProviderId = providerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTransport = await _transportRepo.AddAsync(transport);
            await _transportRepo.SaveChangesAsync();
            return MapToResponse(createdTransport);
        }

        public async Task<TransportBookingResponse> BookTransportAsync(Guid transportId, BookTransportRequest request, string userId)
        {
            var transport = await _transportRepo.GetByIdAsync(transportId);
            if (transport == null)
                throw new Exception("Transport not found");

            if (transport.AvailableSeats < request.NumberOfSeats)
                throw new Exception("Not enough available seats");

            var totalPrice = transport.Price * request.NumberOfSeats;

            var booking = new TransportBooking
            {
                UserId = userId,
                TransportId = transportId,
                NumberOfSeats = request.NumberOfSeats,
                TotalPrice = totalPrice,
                Status = "pending",
                BookingDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepo.AddAsync(booking);
            // Save booking first to ensure it's persisted
            await _bookingRepo.SaveChangesAsync();

            // Now update transport's available seats
            transport.AvailableSeats -= request.NumberOfSeats;
            await _transportRepo.UpdateAsync(transport);
            await _transportRepo.SaveChangesAsync();

            return MapBookingToResponse(createdBooking, transport.Name);
        }

        public async Task<IEnumerable<TransportBookingResponse>> GetUserTransportBookingsAsync(string userId)
        {
            var bookings = await _bookingRepo.GetUserBookingsAsync(userId);
            return bookings.Select(b => MapBookingToResponse(b, b.Transport?.Name ?? "Unknown"));
        }

        public async Task<IEnumerable<TransportBookingResponse>> GetProviderTransportBookingsAsync(string providerId)
        {
            var bookings = await _bookingRepo.GetProviderBookingsAsync(providerId);
            return bookings.Select(b => MapBookingToResponse(b, b.Transport?.Name ?? "Unknown"));
        }

        private TransportResponse MapToResponse(Transport transport)
        {
            return new TransportResponse
            {
                Id = transport.Id,
                Name = transport.Name,
                Type = transport.Type,
                Description = transport.Description,
                ImageUrl = transport.ImageUrl,
                DepartureLocation = transport.DepartureLocation,
                ArrivalLocation = transport.ArrivalLocation,
                DepartureTime = transport.DepartureTime,
                ArrivalTime = transport.ArrivalTime,
                Price = transport.Price,
                AvailableSeats = transport.AvailableSeats,
                TotalCapacity = transport.TotalCapacity,
                Rating = transport.Rating,
                ReviewCount = transport.ReviewCount
            };
        }

        private TransportBookingResponse MapBookingToResponse(TransportBooking booking, string transportName)
        {
            return new TransportBookingResponse
            {
                Id = booking.Id,
                TransportId = booking.TransportId,
                TransportName = transportName,
                NumberOfSeats = booking.NumberOfSeats,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate
            };
        }
    }
}
