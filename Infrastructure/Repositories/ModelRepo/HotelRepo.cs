using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class HotelRepo : Repository<Hotel>, IHotelRepo
    {
        public HotelRepo(AppDbContext context, ILogger<Hotel> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<Hotel>> GetHotelsByProviderAsync(string providerId)
        {
            return await _dbSet.Where(h => h.ProviderId == providerId).ToListAsync();
        }

        public async Task<IEnumerable<Hotel>> GetAvailableHotelsAsync()
        {
            return await _dbSet.Where(h => h.AvailableRooms > 0).ToListAsync();
        }

        public async Task<Hotel?> GetHotelWithBookingsAsync(Guid hotelId)
        {
            return await _dbSet.Include(h => h.Bookings).FirstOrDefaultAsync(h => h.Id == hotelId);
        }
    }
}
