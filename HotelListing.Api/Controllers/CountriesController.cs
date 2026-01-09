using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(ICountriesService service) : ControllerBase
{
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var response = await service.GetCountriesAsync();

        return Ok(response);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var response = await service.GetCountryAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    // POST: api/Countries
    [HttpPost]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto)
    {
        var response = await service.CreateCountryAsync(countryDto);

        return CreatedAtAction("GetCountry", new { id = response.CountryId }, response);
    }

    // PUT: api/Countries/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        if (id != countryDto.CountryId)
        {
            return BadRequest();
        }

        var isUpdated = await service.UpdateCountryAsync(id, countryDto);

        if (!isUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var isDeleted = await service.DeleteCountryAsync(id);

        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}