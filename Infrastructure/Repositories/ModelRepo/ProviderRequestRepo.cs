using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class ProviderRequestRepo : Repository<ProviderRequest>, IProviderRequestRepo
    {
        public ProviderRequestRepo(AppDbContext context, ILogger<ProviderRequest> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<ProviderRequest>> GetPendingRequestsAsync()
        {
            return await _dbSet.Where(r => r.Status == "pending").Include(r => r.User).ToListAsync();
        }

        public async Task<ProviderRequest?> GetRequestByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task<IEnumerable<ProviderRequest>> GetRequestsByStatusAsync(string status)
        {
            return await _dbSet.Where(r => r.Status == status).Include(r => r.User).ToListAsync();
        }
    }
}
