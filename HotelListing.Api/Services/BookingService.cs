using AutoMapper;
using HotelListing.Api.Data;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListing.Api.Services;

public class BookingService(
    IBookingsRepository repository,
    IHotelsRepository hotelsRepository,
    IHttpContextAccessor httpContext
    ) : IBookingsService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExist = await hotelsRepository.Exists(hotelId);

        if (!hotelExist)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound();
        }

        var bookings = await repository.GetBookingsForHotelAsync(hotelId);

        var response = bookings.Select(b => new GetBookingDto(
            b.Id,
            b.HotelId,
            b.Hotel!.Name,
            b.CheckIn,
            b.CheckOut,
            b.Guests,
            b.TotalPrice,
            b.Status.ToString(),
            b.CreatedAtUtc,
            b.UpdatedAtUtc
            ));

        return Result<IEnumerable<GetBookingDto>>.Success(response);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(int hotelId, CreateBookingDto createDto)
    {
        var userId = httpContext?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? httpContext?
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;

        var overlap = await repository.IsOverlap(hotelId, userId, createDto.CheckIn, createDto.CheckOut);

        if (overlap)
        {
            return Result<GetBookingDto>.Conflict("");
        }

        var hotel = await hotelsRepository.GetByIdAsync(hotelId);

        if (hotel==null)
        {
            return Result<GetBookingDto>.NotFound();
        }

        if (hotelId != createDto.HotelId)
        {
            return Result<GetBookingDto>.BadRequest("");
        }

        var nights = createDto.CheckOut.DayNumber - createDto.CheckIn.DayNumber;
        var price = hotel.PerNightRate * nights;

        var booking = new Booking
        {
            HotelId = createDto.HotelId,
            UserId = userId,
            CheckIn = createDto.CheckIn,
            CheckOut = createDto.CheckOut,
            Guests = createDto.Guests,
            TotalPrice = price
        };

        await repository.AddAsync(booking);
        
        booking = await repository.GetBookingDetailsAsync(booking.Id);

        var response = new GetBookingDto(
            booking.Id,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );

        return Result<GetBookingDto>.Success(response);
}    


    public async Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateDto)
    {
        var userId = httpContext?
           .HttpContext?
           .User?
           .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
       ?? httpContext?
           .HttpContext?
           .User?
           .FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? string.Empty;

        var overlap = await repository.IsOverlap(hotelId, userId, updateDto.CheckIn, updateDto.CheckOut, bookingId);

        if (overlap)
        {
            return Result<GetBookingDto>.Conflict("");
        }

        var booking = await repository.GetUserBookingForHotelAsync(bookingId, hotelId, userId);

        if(booking == null)
        {
            return Result<GetBookingDto>.NotFound();
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Conflict("");
        }

        var perNightRate = booking.Hotel!.PerNightRate;
        var nights = updateDto.CheckOut.DayNumber - updateDto.CheckIn.DayNumber;
        var totalPrice = perNightRate * nights;

        booking.TotalPrice = totalPrice;
        booking.CheckIn = updateDto.CheckIn;
        booking.CheckOut = updateDto.CheckOut;
        booking.Guests = updateDto.Guests;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(booking);

        var response = new GetBookingDto(
            booking.Id,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );

        return Result<GetBookingDto>.Success(response);
    }

    public async Task<Result> DeleteBookingAsync(int hotelId, int bookingId)
    {
        var booking = await repository.GetBookingDetailsAsync(bookingId, hotelId);

        if(booking == null)
        {
            return Result.NotFound();
        }

        await repository.DeleteAsync(bookingId);

        return Result.Success();
    }

}
