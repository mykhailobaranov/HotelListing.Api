using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Hotel;

namespace HotelListing.Api.Services;

public interface IHotelsService
{
    Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync();
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);
}