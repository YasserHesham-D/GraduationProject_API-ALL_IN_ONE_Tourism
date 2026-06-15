using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class ProgramRepo : Repository<Program>, IProgramRepo
    {
        public ProgramRepo(AppDbContext context, ILogger<Program> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Program>> GetProgramsByProviderAsync(string providerId)
        {
            return await _dbSet.Where(p => p.ProviderId == providerId).ToListAsync();
        }

        public async Task<IEnumerable<Program>> GetAvailableProgramsAsync()
        {
            return await _dbSet.Where(p => p.AvailableSpots > 0).ToListAsync();
        }

        public async Task<Program?> GetProgramWithBookingsAsync(Guid programId)
        {
            return await _dbSet.Include(p => p.Bookings).FirstOrDefaultAsync(p => p.Id == programId);
        }
    }
}
