using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IGuideBookingRepo : IRepo<GuideBooking>
    {
        Task<IEnumerable<GuideBooking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<GuideBooking>> GetProviderBookingsAsync(string providerId);
        Task<GuideBooking?> GetBookingWithDetailsAsync(Guid bookingId);
    }
}
