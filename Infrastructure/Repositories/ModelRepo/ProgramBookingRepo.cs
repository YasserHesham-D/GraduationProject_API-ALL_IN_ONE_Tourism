using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class ProgramBookingRepo : Repository<ProgramBooking>, IProgramBookingRepo
    {
        public ProgramBookingRepo(AppDbContext context, ILogger<ProgramBooking> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<ProgramBooking>> GetUserBookingsAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).Include(b => b.Program).ToListAsync();
        }

        public async Task<IEnumerable<ProgramBooking>> GetProviderBookingsAsync(string providerId)
        {
            return await _dbSet.Where(b => b.Program!.ProviderId == providerId).Include(b => b.User).Include(b => b.Program).ToListAsync();
        }

        public async Task<ProgramBooking?> GetBookingWithDetailsAsync(Guid bookingId)
        {
            return await _dbSet.Include(b => b.User).Include(b => b.Program).FirstOrDefaultAsync(b => b.Id == bookingId);
        }
    }
}
