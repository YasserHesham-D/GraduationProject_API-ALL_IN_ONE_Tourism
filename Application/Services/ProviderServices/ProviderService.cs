using Application.Dtos.ProviderManagement;
using Application.Services.ProviderServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRequestRepo _providerRequestRepo;
        private readonly IProviderEarningsRepo _earningsRepo;
        private readonly IHotelBookingRepo _hotelBookingRepo;
        private readonly ITransportBookingRepo _transportBookingRepo;
        private readonly IProgramBookingRepo _programBookingRepo;
        private readonly IGuideBookingRepo _guideBookingRepo;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProviderService> _logger;

        public ProviderService(
            IProviderRequestRepo providerRequestRepo,
            IProviderEarningsRepo earningsRepo,
            IHotelBookingRepo hotelBookingRepo,
            ITransportBookingRepo transportBookingRepo,
            IProgramBookingRepo programBookingRepo,
            IGuideBookingRepo guideBookingRepo,
            UserManager<User> userManager,
            ILogger<ProviderService> logger)
        {
            _providerRequestRepo = providerRequestRepo;
            _earningsRepo = earningsRepo;
            _hotelBookingRepo = hotelBookingRepo;
            _transportBookingRepo = transportBookingRepo;
            _programBookingRepo = programBookingRepo;
            _guideBookingRepo = guideBookingRepo;
            _userManager = userManager;
            _logger = logger;
        }

        // Provider Requests
        public async Task<ProviderRequestResponse> SubmitProviderRequestAsync(CreateProviderRequestDto request, string userId)
        {
            // Check if user already has a pending request
            var existingRequest = await _providerRequestRepo.GetRequestByUserIdAsync(userId);
            if (existingRequest != null && existingRequest.Status == "pending")
                throw new Exception("You already have a pending provider request");

            var providerRequest = new ProviderRequest
            {
                UserId = userId,
                BusinessName = request.BusinessName,
                BusinessType = request.BusinessType,
                BusinessDescription = request.BusinessDescription,
                ContactNumber = request.ContactNumber,
                Email = request.Email,
                TaxNumber = request.TaxNumber,
                RegistrationNumber = request.RegistrationNumber,
                DocumentUrl = request.DocumentUrl,
                Status = "pending",
                SubmittedAt = DateTime.UtcNow
            };

            var createdRequest = await _providerRequestRepo.AddAsync(providerRequest);
            await _providerRequestRepo.SaveChangesAsync();
            return MapRequestToResponse(createdRequest);
        }

        public async Task<ProviderRequestResponse?> GetMyProviderRequestAsync(string userId)
        {
            var request = await _providerRequestRepo.GetRequestByUserIdAsync(userId);
            return request != null ? MapRequestToResponse(request) : null;
        }

        public async Task<IEnumerable<ProviderRequestResponse>> GetPendingProviderRequestsAsync()
        {
            var requests = await _providerRequestRepo.GetPendingRequestsAsync();
            return requests.Select(MapRequestToResponse);
        }

        public async Task<ProviderRequestResponse?> ApproveProviderRequestAsync(Guid requestId, string adminId)
        {
            var request = await _providerRequestRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new Exception("Provider request not found");

            request.Status = "approved";
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = adminId;

            await _providerRequestRepo.UpdateAsync(request);
            await _providerRequestRepo.SaveChangesAsync();

            // Get the user and add Provider role
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                
                var result = await _userManager.AddToRoleAsync(user, "Provider");
                if (!result.Succeeded)
                {
                    _logger.LogError($"Failed to add Provider role to user {request.UserId}");
                    throw new Exception($"Failed to assign Provider role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                _logger.LogInformation($"Successfully added Provider role to user {request.UserId}");
            }
            else
            {
                throw new Exception($"User {request.UserId} not found");
            }

            // Create earnings record for provider
            var earnings = new ProviderEarnings
            {
                ProviderId = request.UserId,
                TotalEarnings = 0,
                PendingEarnings = 0,
                WithdrawnAmount = 0,
                CompletedBookings = 0,
                LastUpdated = DateTime.UtcNow
            };
            await _earningsRepo.AddAsync(earnings);
            await _earningsRepo.SaveChangesAsync();

            return MapRequestToResponse(request);
        }

        public async Task<ProviderRequestResponse?> RejectProviderRequestAsync(Guid requestId, string rejectionReason, string adminId)
        {
            var request = await _providerRequestRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new Exception("Provider request not found");

            request.Status = "rejected";
            request.RejectionReason = rejectionReason;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedBy = adminId;

            await _providerRequestRepo.UpdateAsync(request);
            await _providerRequestRepo.SaveChangesAsync();

            return MapRequestToResponse(request);
        }

        // Provider Earnings
        public async Task<ProviderEarningsResponse?> GetProviderEarningsAsync(string providerId)
        {
            var earnings = await _earningsRepo.GetEarningsByProviderIdAsync(providerId);
            return earnings != null ? MapEarningsToResponse(earnings) : null;
        }

        // Provider Bookings
        public async Task<bool> ConfirmBookingAsync(Guid bookingId, string providerId)
        {
            // Try all booking types
            var hotelBooking = await _hotelBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (hotelBooking != null && hotelBooking.Hotel?.ProviderId == providerId)
            {
                hotelBooking.Status = "confirmed";
                hotelBooking.UpdatedAt = DateTime.UtcNow;
                await _hotelBookingRepo.UpdateAsync(hotelBooking);
                await _hotelBookingRepo.SaveChangesAsync();
                return true;
            }

            var transportBooking = await _transportBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (transportBooking != null && transportBooking.Transport?.ProviderId == providerId)
            {
                transportBooking.Status = "confirmed";
                transportBooking.UpdatedAt = DateTime.UtcNow;
                await _transportBookingRepo.UpdateAsync(transportBooking);
                await _transportBookingRepo.SaveChangesAsync();
                return true;
            }

            var programBooking = await _programBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (programBooking != null && programBooking.Program?.ProviderId == providerId)
            {
                programBooking.Status = "confirmed";
                programBooking.UpdatedAt = DateTime.UtcNow;
                await _programBookingRepo.UpdateAsync(programBooking);
                await _programBookingRepo.SaveChangesAsync();
                return true;
            }

            var guideBooking = await _guideBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (guideBooking != null && guideBooking.Guide?.ProviderId == providerId)
            {
                guideBooking.Status = "confirmed";
                guideBooking.UpdatedAt = DateTime.UtcNow;
                await _guideBookingRepo.UpdateAsync(guideBooking);
                await _guideBookingRepo.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DeclineBookingAsync(Guid bookingId, string providerId)
        {
            var hotelBooking = await _hotelBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (hotelBooking != null && hotelBooking.Hotel?.ProviderId == providerId)
            {
                hotelBooking.Status = "declined";
                hotelBooking.UpdatedAt = DateTime.UtcNow;
                await _hotelBookingRepo.UpdateAsync(hotelBooking);
                await _hotelBookingRepo.SaveChangesAsync();
                return true;
            }

            var transportBooking = await _transportBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (transportBooking != null && transportBooking.Transport?.ProviderId == providerId)
            {
                transportBooking.Status = "declined";
                transportBooking.UpdatedAt = DateTime.UtcNow;
                await _transportBookingRepo.UpdateAsync(transportBooking);
                await _transportBookingRepo.SaveChangesAsync();
                return true;
            }

            var programBooking = await _programBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (programBooking != null && programBooking.Program?.ProviderId == providerId)
            {
                programBooking.Status = "declined";
                programBooking.UpdatedAt = DateTime.UtcNow;
                await _programBookingRepo.UpdateAsync(programBooking);
                await _programBookingRepo.SaveChangesAsync();
                return true;
            }

            var guideBooking = await _guideBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (guideBooking != null && guideBooking.Guide?.ProviderId == providerId)
            {
                guideBooking.Status = "declined";
                guideBooking.UpdatedAt = DateTime.UtcNow;
                await _guideBookingRepo.UpdateAsync(guideBooking);
                await _guideBookingRepo.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> CompleteBookingAsync(Guid bookingId, string providerId)
        {
            var hotelBooking = await _hotelBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (hotelBooking != null && hotelBooking.Hotel?.ProviderId == providerId)
            {
                hotelBooking.Status = "completed";
                hotelBooking.UpdatedAt = DateTime.UtcNow;
                await _hotelBookingRepo.UpdateAsync(hotelBooking);
                await _hotelBookingRepo.SaveChangesAsync();

                // Update earnings
                await _earningsRepo.UpdateEarningsAsync(providerId, hotelBooking.TotalPrice, false);
                return true;
            }

            var transportBooking = await _transportBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (transportBooking != null && transportBooking.Transport?.ProviderId == providerId)
            {
                transportBooking.Status = "completed";
                transportBooking.UpdatedAt = DateTime.UtcNow;
                await _transportBookingRepo.UpdateAsync(transportBooking);
                await _transportBookingRepo.SaveChangesAsync();

                await _earningsRepo.UpdateEarningsAsync(providerId, transportBooking.TotalPrice, false);
                return true;
            }

            var programBooking = await _programBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (programBooking != null && programBooking.Program?.ProviderId == providerId)
            {
                programBooking.Status = "completed";
                programBooking.UpdatedAt = DateTime.UtcNow;
                await _programBookingRepo.UpdateAsync(programBooking);
                await _programBookingRepo.SaveChangesAsync();

                await _earningsRepo.UpdateEarningsAsync(providerId, programBooking.TotalPrice, false);
                return true;
            }

            var guideBooking = await _guideBookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (guideBooking != null && guideBooking.Guide?.ProviderId == providerId)
            {
                guideBooking.Status = "completed";
                guideBooking.UpdatedAt = DateTime.UtcNow;
                await _guideBookingRepo.UpdateAsync(guideBooking);
                await _guideBookingRepo.SaveChangesAsync();

                await _earningsRepo.UpdateEarningsAsync(providerId, guideBooking.TotalPrice, false);
                return true;
            }

            return false;
        }

        public async Task<bool> ContactUserAsync(Guid bookingId, string providerId, string message)
        {
            // This is a placeholder for contact logic
            // In a real scenario, you'd send an email or notification
            _logger.LogInformation($"Provider {providerId} sent message for booking {bookingId}: {message}");
            return true;
        }

        private ProviderRequestResponse MapRequestToResponse(ProviderRequest request)
        {
            return new ProviderRequestResponse
            {
                Id = request.Id,
                BusinessName = request.BusinessName,
                BusinessType = request.BusinessType,
                Status = request.Status,
                SubmittedAt = request.SubmittedAt,
                ReviewedAt = request.ReviewedAt,
                RejectionReason = request.RejectionReason
            };
        }

        private ProviderEarningsResponse MapEarningsToResponse(ProviderEarnings earnings)
        {
            return new ProviderEarningsResponse
            {
                Id = earnings.Id,
                TotalEarnings = earnings.TotalEarnings,
                PendingEarnings = earnings.PendingEarnings,
                WithdrawnAmount = earnings.WithdrawnAmount,
                CompletedBookings = earnings.CompletedBookings,
                LastUpdated = earnings.LastUpdated
            };
        }
    }
}
