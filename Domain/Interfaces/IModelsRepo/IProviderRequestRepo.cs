using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IProviderRequestRepo : IRepo<ProviderRequest>
    {
        Task<IEnumerable<ProviderRequest>> GetPendingRequestsAsync();
        Task<ProviderRequest?> GetRequestByUserIdAsync(string userId);
        Task<IEnumerable<ProviderRequest>> GetRequestsByStatusAsync(string status);
    }
}
