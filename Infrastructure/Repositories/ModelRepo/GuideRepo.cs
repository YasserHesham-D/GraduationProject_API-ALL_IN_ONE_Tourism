using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class GuideRepo : Repository<Guide>, IGuideRepo
    {
        public GuideRepo(AppDbContext context, ILogger<Guide> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Guide>> GetGuidesByProviderAsync(string providerId)
        {
            return await _dbSet.Where(g => g.ProviderId == providerId).ToListAsync();
        }

        public async Task<IEnumerable<Guide>> GetAvailableGuidesAsync()
        {
            return await _dbSet.Where(g => g.IsAvailable).ToListAsync();
        }

        public async Task<Guide?> GetGuideWithBookingsAsync(Guid guideId)
        {
            return await _dbSet.Include(g => g.Bookings).FirstOrDefaultAsync(g => g.Id == guideId);
        }
    }
}
