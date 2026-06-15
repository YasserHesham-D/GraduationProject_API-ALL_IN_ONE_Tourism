using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IHotelBookingRepo : IRepo<HotelBooking>
    {
        Task<IEnumerable<HotelBooking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<HotelBooking>> GetProviderBookingsAsync(string providerId);
        Task<HotelBooking?> GetBookingWithDetailsAsync(Guid bookingId);
    }
}
