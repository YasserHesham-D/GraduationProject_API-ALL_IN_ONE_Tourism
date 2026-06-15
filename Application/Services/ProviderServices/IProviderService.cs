using Application.Dtos.ProviderManagement;

namespace Application.Services.ProviderServices
{
    public interface IProviderService
    {
        // Provider Requests
        Task<ProviderRequestResponse> SubmitProviderRequestAsync(CreateProviderRequestDto request, string userId);
        Task<ProviderRequestResponse?> GetMyProviderRequestAsync(string userId);
        Task<IEnumerable<ProviderRequestResponse>> GetPendingProviderRequestsAsync();
        Task<ProviderRequestResponse?> ApproveProviderRequestAsync(Guid requestId, string adminId);
        Task<ProviderRequestResponse?> RejectProviderRequestAsync(Guid requestId, string rejectionReason, string adminId);

        // Provider Earnings
        Task<ProviderEarningsResponse?> GetProviderEarningsAsync(string providerId);

        // Provider Bookings
        Task<bool> ConfirmBookingAsync(Guid bookingId, string providerId);
        Task<bool> DeclineBookingAsync(Guid bookingId, string providerId);
        Task<bool> CompleteBookingAsync(Guid bookingId, string providerId);
        Task<bool> ContactUserAsync(Guid bookingId, string providerId, string message);
    }
}
