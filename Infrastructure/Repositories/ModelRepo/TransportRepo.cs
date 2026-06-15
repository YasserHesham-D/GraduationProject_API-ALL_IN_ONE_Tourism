using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class TransportRepo : Repository<Transport>, ITransportRepo
    {
        public TransportRepo(AppDbContext context, ILogger<Transport> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Transport>> GetTransportByProviderAsync(string providerId)
        {
            return await _dbSet.Where(t => t.ProviderId == providerId).ToListAsync();
        }

        public async Task<IEnumerable<Transport>> GetAvailableTransportAsync()
        {
            return await _dbSet.Where(t => t.AvailableSeats > 0).ToListAsync();
        }

        public async Task<Transport?> GetTransportWithBookingsAsync(Guid transportId)
        {
            return await _dbSet.Include(t => t.Bookings).FirstOrDefaultAsync(t => t.Id == transportId);
        }
    }
}
