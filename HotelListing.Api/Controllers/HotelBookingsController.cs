using Asp.Versioning;
using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Models.Constants;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

[Route("api/v{version:apiVersion}/hotels/{hotelId:int}/bookings")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
[EnableRateLimiting("perUser")]
public class HotelBookingsController(IBookingsService service) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetUserBookings(
        [FromRoute] int hotelId, [FromQuery] PaginationParameters paging, [FromQuery] BookingFilterParameters filters)
    {
        var result = await service.GetUserBookingsForHotelAsync(hotelId, paging, filters);
        return HandleResult(result);
    }

    [HttpGet("admin")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetHotelBookings(
        [FromRoute] int hotelId, [FromQuery] PaginationParameters paging, [FromQuery] BookingFilterParameters filters)
    {
        var result = await service.GetBookingsForHotelAsync(hotelId, paging, filters);
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
    public async Task<ActionResult<GetBookingDetailsDto>> UpdateBooking([FromRoute] int hotelId, [FromRoute] int bookingId, [FromBody] UpdateBookingDto updateDto)
    {
        var result = await service.UpdateBookingAsync(hotelId, bookingId, updateDto);
        return HandleResult(result);
    }

    [HttpDelete("{bookingId:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.DeleteBookingAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<IActionResult> CancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.CancelBookingAsync(hotelId, bookingId);
        return HandleResult(result);
    }

    [HttpPost("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdmin]
    public async Task<IActionResult> AdminCanelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.AdminUpdateBookingStatusAsync(hotelId, bookingId, BookingStatus.Cancelled);
        return HandleResult(result);
    }

    [HttpPost("{bookingId:int}/admin/confirm")]
    [HotelOrSystemAdmin]
    public async Task<IActionResult> ConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await service.AdminUpdateBookingStatusAsync(hotelId, bookingId, BookingStatus.Confirmed);
        return HandleResult(result);
    }
}