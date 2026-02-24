using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
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

    [HttpGet("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDetailsDto>> GetUserBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.GetUserBookingDetailsAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpGet("{bookingId:int}/admin")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<GetAdminBookingDetailsDto>> GetUserBookingAdmin([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.GetAdminBookingDetailsAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto createDto)
    {
        var result = await service.CreateBookingAsync(hotelId, createDto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetUserBooking),
                new { hotelId, bookingId = result.Value!.Id },
                result.Value
            );
        }

        return HandleResult(result);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking([FromRoute] int hotelId, [FromRoute] int bookingId, [FromBody] UpdateBookingDto updateDto)
    {
        var result = await service.UpdateBookingAsync(hotelId, bookingId, updateDto);
        return HandleResult(result);
    }

    [HttpDelete("{bookingId:int}")]
    [Authorize(RoleNames.Admin)]
    public async Task<ActionResult> DeleteBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.DeleteBookingAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult> CancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.CancelBookingAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpPut("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<GetBookingDto>> AdminCanelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.AdminUpdateBookingStatusAsync(hotelId, bookingId, BookingStatus.Cancelled);
        return HandleResult(result);
    }

    [HttpPut("{bookingId:int}/admin/confirm")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<GetBookingDto>> ConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.AdminUpdateBookingStatusAsync(hotelId, bookingId, BookingStatus.Confirmed);
        return HandleResult(result);
    }
}
