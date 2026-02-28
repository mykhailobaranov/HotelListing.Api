using HotelListing.Api.Models.Constants;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

[Route("api/hotels")]
[ApiController]
[EnableRateLimiting("fixed")]
public class HotelsController(IHotelsService service, IOutputCacheStore cacheStore) : BaseApiController
{
    [HttpGet]
    [OutputCache(Duration = 60, Tags = new[] { "hotels" })]
    public async Task<ActionResult<PagedResult<GetHotelsDto>>> GetHotels(
        [FromQuery] PaginationParameters paging, [FromQuery] HotelFilterParameters filters)
    {
        var result = await service.GetHotelsAsync(paging, filters);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [OutputCache(Duration = 60, Tags = new[] { "hotels" })]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var result = await service.GetHotelAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto, CancellationToken cancellationToken)
    {
        var result = await service.CreateHotelAsync(hotelDto);

        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("hotels", cancellationToken);
            return CreatedAtAction(nameof(GetHotel), new { id = result.Value!.Id }, result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto, CancellationToken cancellationToken)
    {
        var result = await service.UpdateHotelAsync(id, hotelDto);

        if(result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("hotels", cancellationToken);
        }

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteHotel(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteHotelAsync(id);

        if(result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync("hotels", cancellationToken);
        }

        return HandleResult(result);
    }
}