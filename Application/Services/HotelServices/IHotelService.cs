using Application.Dtos.Hotels;

namespace Application.Services.HotelServices
{
    public interface IHotelService
    {
        Task<IEnumerable<HotelResponse>> GetAllHotelsAsync();
        Task<HotelResponse?> GetHotelByIdAsync(Guid id);
        Task<HotelResponse> CreateHotelAsync(CreateHotelRequest request, string providerId);
        Task<HotelBookingResponse> BookHotelAsync(Guid hotelId, BookHotelRequest request, string userId);
        Task<IEnumerable<HotelBookingResponse>> GetUserHotelBookingsAsync(string userId);
        Task<IEnumerable<HotelBookingResponse>> GetProviderHotelBookingsAsync(string providerId);
    }
}
