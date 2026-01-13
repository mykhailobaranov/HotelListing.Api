using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Country;

namespace HotelListing.Api.Services
{
    public interface ICountriesService
    {
        Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync();
        Task<Result<GetCountryDto>> GetCountryAsync(int id);
        Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
        Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
        Task<Result> DeleteCountryAsync(int id);
    }
}
