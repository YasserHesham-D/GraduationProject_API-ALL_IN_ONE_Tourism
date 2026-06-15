using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IGuideRepo : IRepo<Guide>
    {
        Task<IEnumerable<Guide>> GetGuidesByProviderAsync(string providerId);
        Task<IEnumerable<Guide>> GetAvailableGuidesAsync();
        Task<Guide?> GetGuideWithBookingsAsync(Guid guideId);
    }
}
