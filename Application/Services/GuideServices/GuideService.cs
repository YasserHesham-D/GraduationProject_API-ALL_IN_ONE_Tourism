using Application.Dtos.Guides;
using Application.Services.GuideServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class GuideService : IGuideService
    {
        private readonly IGuideRepo _guideRepo;
        private readonly IGuideBookingRepo _bookingRepo;
        private readonly ILogger<GuideService> _logger;

        public GuideService(IGuideRepo guideRepo, IGuideBookingRepo bookingRepo, ILogger<GuideService> logger)
        {
            _guideRepo = guideRepo;
            _bookingRepo = bookingRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<GuideResponse>> GetAllGuidesAsync()
        {
            var guides = await _guideRepo.GetAvailableGuidesAsync();
            return guides.Select(MapToResponse);
        }

        public async Task<GuideResponse?> GetGuideByIdAsync(Guid id)
        {
            var guide = await _guideRepo.GetByIdAsync(id);
            return guide != null ? MapToResponse(guide) : null;
        }

        public async Task<GuideResponse> CreateGuideAsync(CreateGuideRequest request, string providerId)
        {
            var guide = new Guide
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Description = request.Description,
                Nationality = request.Nationality,
                Languages = request.Languages,
                Specialization = request.Specialization,
                ImageUrl = request.ImageUrl,
                Bio = request.Bio,
                PricePerDay = request.PricePerDay,
                IsAvailable = true,
                ProviderId = providerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdGuide = await _guideRepo.AddAsync(guide);
            await _guideRepo.SaveChangesAsync();
            return MapToResponse(createdGuide);
        }

        public async Task<GuideBookingResponse> BookGuideAsync(Guid guideId, BookGuideRequest request, string userId)
        {
            var guide = await _guideRepo.GetByIdAsync(guideId);
            if (guide == null)
                throw new Exception("Guide not found");

            if (!guide.IsAvailable)
                throw new Exception("Guide is not available");

            var numberOfDays = (int)(request.EndDate - request.StartDate).TotalDays;
            var totalPrice = guide.PricePerDay * numberOfDays * request.NumberOfPeople;

            var booking = new GuideBooking
            {
                UserId = userId,
                GuideId = guideId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                NumberOfDays = numberOfDays,
                NumberOfPeople = request.NumberOfPeople,
                TotalPrice = totalPrice,
                Status = "pending",
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return MapBookingToResponse(createdBooking, guide.FullName);
        }

        public async Task<IEnumerable<GuideBookingResponse>> GetUserGuideBookingsAsync(string userId)
        {
            var bookings = await _bookingRepo.GetUserBookingsAsync(userId);
            return bookings.Select(b => MapBookingToResponse(b, b.Guide?.FullName ?? "Unknown"));
        }

        public async Task<IEnumerable<GuideBookingResponse>> GetProviderGuideBookingsAsync(string providerId)
        {
            var bookings = await _bookingRepo.GetProviderBookingsAsync(providerId);
            return bookings.Select(b => MapBookingToResponse(b, b.Guide?.FullName ?? "Unknown"));
        }

        private GuideResponse MapToResponse(Guide guide)
        {
            return new GuideResponse
            {
                Id = guide.Id,
                FullName = guide.FullName,
                PhoneNumber = guide.PhoneNumber,
                Email = guide.Email,
                Description = guide.Description,
                Nationality = guide.Nationality,
                Languages = guide.Languages,
                Specialization = guide.Specialization,
                ImageUrl = guide.ImageUrl,
                Bio = guide.Bio,
                PricePerDay = guide.PricePerDay,
                Rating = guide.Rating,
                ReviewCount = guide.ReviewCount,
                IsAvailable = guide.IsAvailable
            };
        }

        private GuideBookingResponse MapBookingToResponse(GuideBooking booking, string guideName)
        {
            return new GuideBookingResponse
            {
                Id = booking.Id,
                GuideId = booking.GuideId,
                GuideName = guideName,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                NumberOfPeople = booking.NumberOfPeople,
                NumberOfDays = booking.NumberOfDays,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }
    }
}
