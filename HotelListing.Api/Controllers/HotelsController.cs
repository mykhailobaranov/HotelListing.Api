using HotelListing.Api.Models.Constants;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels")]
[ApiController]
[Authorize]
public class HotelsController(IHotelsService service) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetHotelsDto>>> GetHotels([FromQuery] PaginationParameters parameters)
    {
        var result = await service.GetHotelsAsync(parameters);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var result = await service.GetHotelAsync(id);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var result = await service.CreateHotelAsync(hotelDto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetHotel), new { id = result.Value!.Id }, result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        var result = await service.UpdateHotelAsync(id, hotelDto);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await service.DeleteHotelAsync(id);
        return HandleResult(result);
    }
}