using Application.Dtos.Guides;

namespace Application.Services.GuideServices
{
    public interface IGuideService
    {
        Task<IEnumerable<GuideResponse>> GetAllGuidesAsync();
        Task<GuideResponse?> GetGuideByIdAsync(Guid id);
        Task<GuideResponse> CreateGuideAsync(CreateGuideRequest request, string providerId);
        Task<GuideBookingResponse> BookGuideAsync(Guid guideId, BookGuideRequest request, string userId);
        Task<IEnumerable<GuideBookingResponse>> GetUserGuideBookingsAsync(string userId);
        Task<IEnumerable<GuideBookingResponse>> GetProviderGuideBookingsAsync(string providerId);
    }
}
