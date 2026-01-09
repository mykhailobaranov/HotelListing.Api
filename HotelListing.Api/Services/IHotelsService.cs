using HotelListing.Api.Models.DTOs.Hotel;

namespace HotelListing.Api.Services
{
    public interface IHotelsService
    {
        Task<IEnumerable<GetHotelsDto>> GetHotelsAsync();
        Task<GetHotelDto?> GetHotelAsync(int id);
        Task<GetHotelDto> CreateHotelAsync(CreateHotelDto hotelDto);
        Task<bool> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
        Task<bool> DeleteHotelAsync(int id);
    }
}
