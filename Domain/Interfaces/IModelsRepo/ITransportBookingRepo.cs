using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface ITransportBookingRepo : IRepo<TransportBooking>
    {
        Task<IEnumerable<TransportBooking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<TransportBooking>> GetProviderBookingsAsync(string providerId);
        Task<TransportBooking?> GetBookingWithDetailsAsync(Guid bookingId);
    }
}
