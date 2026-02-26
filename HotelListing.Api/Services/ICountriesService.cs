using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace HotelListing.Api.Services;

public interface ICountriesService
{
    Task<Result<PagedResult<GetCountriesDto>>> GetCountriesAsync(PaginationParameters paging, CountryFilterParameters filters);
    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
    Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> countryDto);
    Task<Result> DeleteCountryAsync(int id);
}