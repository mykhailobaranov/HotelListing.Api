using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Models.Pagination;

namespace HotelListing.Api.Services;

public interface IHotelsService
{
    Task<Result<PagedResult<GetHotelsDto>>> GetHotelsAsync(PaginationParameters parameters);
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);
}