using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.ModelRepo
{
    public class HotelBookingRepo : Repository<HotelBooking>, IHotelBookingRepo
    {
        public HotelBookingRepo(AppDbContext context, ILogger<HotelBooking> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<HotelBooking>> GetUserBookingsAsync(string userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).Include(b => b.Hotel).ToListAsync();
        }

        public async Task<IEnumerable<HotelBooking>> GetProviderBookingsAsync(string providerId)
        {
            return await _dbSet.Where(b => b.Hotel!.ProviderId == providerId).Include(b => b.User).Include(b => b.Hotel).ToListAsync();
        }

        public async Task<HotelBooking?> GetBookingWithDetailsAsync(Guid bookingId)
        {
            return await _dbSet.Include(b => b.User).Include(b => b.Hotel).FirstOrDefaultAsync(b => b.Id == bookingId);
        }
    }
}
