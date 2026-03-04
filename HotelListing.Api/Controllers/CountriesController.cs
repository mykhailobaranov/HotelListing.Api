using Asp.Versioning;
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

[Route("api/v{version:apiVersion}/countries")]
[ApiVersion("1.0")]
[ApiController]
[EnableRateLimiting("fixed")]
public class CountriesController(ICountriesService service, IOutputCacheStore cacheStore) : BaseApiController
{
    /// <summary>
    /// Gets a paginated and filtered list of countries.
    /// </summary>
    [HttpGet]
    [OutputCache(Duration = 60, Tags = new[] { "countries" })]
    [ProducesResponseType(typeof(PagedResult<GetCountriesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GetCountriesDto>>> GetCountries(
        [FromQuery] PaginationParameters paging, [FromQuery] CountryFilterParameters filters)
    {
        var result = await service.GetCountriesAsync(paging, filters);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a specific country by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the country.</param>
    [HttpGet("{id}")]
    [OutputCache(Duration = 60, Tags = new[] { "countries" })]
    [ProducesResponseType(typeof(GetCountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await service.GetCountryAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new country (Admin only).
    /// </summary>
    /// <param name="countryDto">The data for the new country.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(GetCountryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Updates an entire country record by its ID (Admin only).
    /// </summary>
    /// <param name="id">The unique identifier of the country to update.</param>
    /// <param name="countryDto">The updated country data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto, CancellationToken cancellationToken)
    {
        var result = await service.UpdateCountryAsync(id, countryDto);
        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("countries", cancellationToken);
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Partially updates a country record using a JSON Patch document (Admin only).
    /// </summary>
    /// <param name="id">The unique identifier of the country to patch.</param>
    /// <param name="patchDoc">The JSON Patch document containing the changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Deletes a country by its ID (Admin only).
    /// </summary>
    /// <param name="id">The unique identifier of the country to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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