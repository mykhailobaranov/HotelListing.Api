using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
public class HotelBookingsController(IBookingsService service) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetUserBookings([FromRoute] int hotelId)
    {
        var result = await service.GetUserBookingsForHotelAsync(hotelId);
        return HandleResult(result);
    }

    [HttpGet("admin")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetHotelBookings([FromRoute] int hotelId)
    {
        var result = await service.GetBookingsForHotelAsync(hotelId);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto createDto)
    {
        var result = await service.CreateBookingAsync(hotelId, createDto);
        return HandleResult(result);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking([FromRoute] int hotelId, [FromRoute] int bookingId, [FromBody] UpdateBookingDto updateDto)
    {
        var result = await service.UpdateBookingAsync(hotelId, bookingId, updateDto);
        return HandleResult(result);
    }

    [HttpDelete("{bookingId:int}")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult> DeleteBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.DeleteBookingAsync(hotelId, bookingId);
        return HandleResult(result);
    }
}
