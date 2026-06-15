using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface ITransportRepo : IRepo<Transport>
    {
        Task<IEnumerable<Transport>> GetTransportByProviderAsync(string providerId);
        Task<IEnumerable<Transport>> GetAvailableTransportAsync();
        Task<Transport?> GetTransportWithBookingsAsync(Guid transportId);
    }
}
