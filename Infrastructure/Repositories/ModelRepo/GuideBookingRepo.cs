using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class GuideBookingRepo : Repository<GuideBooking>, IGuideBookingRepo
    {
        public GuideBookingRepo(AppDbContext context, ILogger<GuideBooking> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<GuideBooking>> GetUserBookingsAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).Include(b => b.Guide).ToListAsync();
        }

        public async Task<IEnumerable<GuideBooking>> GetProviderBookingsAsync(string providerId)
        {
            return await _dbSet.Where(b => b.Guide!.ProviderId == providerId).Include(b => b.User).Include(b => b.Guide).ToListAsync();
        }

        public async Task<GuideBooking?> GetBookingWithDetailsAsync(Guid bookingId)
        {
            return await _dbSet.Include(b => b.User).Include(b => b.Guide).FirstOrDefaultAsync(b => b.Id == bookingId);
        }
    }
}
