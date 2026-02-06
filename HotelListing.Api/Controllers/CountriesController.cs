using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CountriesController(ICountriesService service) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var result = await service.GetCountriesAsync();
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await service.GetCountryAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto)
    {
        var result = await service.CreateCountryAsync(countryDto);

        if(result.IsSuccess)
        {
            return CreatedAtAction("GetCountry", new { id = result.Value!.CountryId }, result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        var result = await service.UpdateCountryAsync(id, countryDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await service.DeleteCountryAsync(id);
        return HandleResult(result);
    }
}