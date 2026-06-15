using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class ProviderEarningsRepo : Repository<ProviderEarnings>, IProviderEarningsRepo
    {
        public ProviderEarningsRepo(AppDbContext context, ILogger<ProviderEarnings> logger) : base(context, logger)
        {
        }

        public async Task<ProviderEarnings?> GetEarningsByProviderIdAsync(string providerId)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.ProviderId == providerId);
        }

        public async Task UpdateEarningsAsync(string providerId, decimal amount, bool isPending = true)
        {
            var earnings = await GetEarningsByProviderIdAsync(providerId);
            if (earnings != null)
            {
                if (isPending)
                    earnings.PendingEarnings += amount;
                else
                    earnings.TotalEarnings += amount;

                earnings.LastUpdated = DateTime.UtcNow;
                _dbSet.Update(earnings);
                await SaveChangesAsync();
            }
        }
    }
}
