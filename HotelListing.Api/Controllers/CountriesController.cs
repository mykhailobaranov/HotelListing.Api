using HotelListing.Api.Models.Constants;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

[Route("api/countries")]
[ApiController]
[EnableRateLimiting("fixed")]
public class CountriesController(ICountriesService service, IOutputCacheStore cacheStore) : BaseApiController
{
    [HttpGet]
    [OutputCache(Duration = 60, Tags = new[] { "countries" })]
    public async Task<ActionResult<PagedResult<GetCountriesDto>>> GetCountries(
        [FromQuery] PaginationParameters paging, [FromQuery] CountryFilterParameters filters)
    {
        var result = await service.GetCountriesAsync(paging, filters);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [OutputCache(Duration = 60, Tags = new[] { "countries" })]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await service.GetCountryAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto, CancellationToken cancellationToken)
    {
        var result = await service.CreateCountryAsync(countryDto);

        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("countries", cancellationToken);
            return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.CountryId }, result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto, CancellationToken cancellationToken)
    {
        var result = await service.UpdateCountryAsync(id, countryDto);
        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("countries", cancellationToken);
        }
        return HandleResult(result);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PatchCountry(int id, [FromBody] JsonPatchDocument<UpdateCountryDto> patchDoc, CancellationToken cancellationToken)
    {
        if (patchDoc == null)
        {
            return BadRequest("Patch document is required.");
        }

        var result = await service.PatchCountryAsync(id, patchDoc);
        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("countries", cancellationToken);
        }
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteCountry(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteCountryAsync(id);
        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("countries", cancellationToken);
        }
        return HandleResult(result);
    }
}