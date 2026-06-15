using Application.Dtos.Transport;

namespace Application.Services.TransportServices
{
    public interface ITransportService
    {
        Task<IEnumerable<TransportResponse>> GetAllTransportsAsync();
        Task<TransportResponse?> GetTransportByIdAsync(Guid id);
        Task<TransportResponse> CreateTransportAsync(CreateTransportRequest request, string providerId);
        Task<TransportBookingResponse> BookTransportAsync(Guid transportId, BookTransportRequest request, string userId);
        Task<IEnumerable<TransportBookingResponse>> GetUserTransportBookingsAsync(string userId);
        Task<IEnumerable<TransportBookingResponse>> GetProviderTransportBookingsAsync(string providerId);
    }
}
