using HotelListing.Api.Models.DTOs.Country;

namespace HotelListing.Api.Services
{
    public interface ICountriesService
    {
        Task<IEnumerable<GetCountriesDto>> GetCountriesAsync();
        Task<GetCountryDto?> GetCountryAsync(int id);
        Task<GetCountryDto> CreateCountryAsync(CreateCountryDto countryDto);
        Task<bool> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
        Task<bool> DeleteCountryAsync(int id);
    }
}
