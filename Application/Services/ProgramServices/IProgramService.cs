using Application.Dtos.Programs;

namespace Application.Services.ProgramServices
{
    public interface IProgramService
    {
        Task<IEnumerable<ProgramResponse>> GetAllProgramsAsync();
        Task<ProgramResponse?> GetProgramByIdAsync(Guid id);
        Task<ProgramResponse> CreateProgramAsync(CreateProgramRequest request, string providerId);
        Task<ProgramBookingResponse> BookProgramAsync(Guid programId, BookProgramRequest request, string userId);
        Task<IEnumerable<ProgramBookingResponse>> GetUserProgramBookingsAsync(string userId);
        Task<IEnumerable<ProgramBookingResponse>> GetProviderProgramBookingsAsync(string providerId);
    }
}
