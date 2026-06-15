using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class TransportBookingRepo : Repository<TransportBooking>, ITransportBookingRepo
    {
        public TransportBookingRepo(AppDbContext context, ILogger<TransportBooking> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<TransportBooking>> GetUserBookingsAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).Include(b => b.Transport).ToListAsync();
        }

        public async Task<IEnumerable<TransportBooking>> GetProviderBookingsAsync(string providerId)
        {
            return await _dbSet.Where(b => b.Transport!.ProviderId == providerId).Include(b => b.User).Include(b => b.Transport).ToListAsync();
        }

        public async Task<TransportBooking?> GetBookingWithDetailsAsync(Guid bookingId)
        {
            return await _dbSet.Include(b => b.User).Include(b => b.Transport).FirstOrDefaultAsync(b => b.Id == bookingId);
        }
    }
}
