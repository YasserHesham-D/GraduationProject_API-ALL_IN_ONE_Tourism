using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IProgramBookingRepo : IRepo<ProgramBooking>
    {
        Task<IEnumerable<ProgramBooking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<ProgramBooking>> GetProviderBookingsAsync(string providerId);
        Task<ProgramBooking?> GetBookingWithDetailsAsync(Guid bookingId);
    }
}
