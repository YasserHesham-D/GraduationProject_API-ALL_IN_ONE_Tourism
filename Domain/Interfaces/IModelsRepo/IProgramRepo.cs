using Domain.Interfaces.IRepository;
using Domain.Models;

namespace Domain.Interfaces.IModelsRepo
{
    public interface IProgramRepo : IRepo<Program>
    {
        Task<IEnumerable<Program>> GetProgramsByProviderAsync(string providerId);
        Task<IEnumerable<Program>> GetAvailableProgramsAsync();
        Task<Program?> GetProgramWithBookingsAsync(Guid programId);
    }
}
