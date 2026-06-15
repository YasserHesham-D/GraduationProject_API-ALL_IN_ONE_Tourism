using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IProviderEarningsRepo : IRepo<ProviderEarnings>
    {
        Task<ProviderEarnings?> GetEarningsByProviderIdAsync(string providerId);
        Task UpdateEarningsAsync(string providerId, decimal amount, bool isPending = true);
    }
}
