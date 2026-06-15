using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IHotelRepo : IRepo<Hotel>
    {
        Task<IEnumerable<Hotel>> GetHotelsByProviderAsync(string providerId);
        Task<IEnumerable<Hotel>> GetAvailableHotelsAsync();
        Task<Hotel?> GetHotelWithBookingsAsync(Guid hotelId);
    }
}
