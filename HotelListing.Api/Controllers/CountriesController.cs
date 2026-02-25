using HotelListing.Api.Models.Constants;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/countries")]
[ApiController]
[Authorize]
public class CountriesController(ICountriesService service) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetCountriesDto>>> GetCountries([FromQuery] PaginationParameters parameters)
    {
        var result = await service.GetCountriesAsync(parameters);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await service.GetCountryAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto)
    {
        var result = await service.CreateCountryAsync(countryDto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.CountryId }, result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        var result = await service.UpdateCountryAsync(id, countryDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await service.DeleteCountryAsync(id);
        return HandleResult(result);
    }
}