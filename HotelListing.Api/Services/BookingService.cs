using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Repositories.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListing.Api.Services;

public class BookingService(
    IBookingsRepository repository,
    IHotelsRepository hotelsRepository,
    IHttpContextAccessor httpContext
    ) : IBookingsService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId)
    {
        var userId = GetUserId();

        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
        }

        var bookings = await repository.GetUserBookingsForHotelAsync(hotelId, userId);

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

    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<IEnumerable<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
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
        var userId = GetUserId();

        if (hotelId != createDto.HotelId)
        {
            return Result<GetBookingDto>.BadRequest($"Route HotelId {hotelId} does not match Body HotelId {createDto.HotelId}.");
        }

        var overlap = await repository.IsOverlapAsync(hotelId, userId, createDto.CheckIn, createDto.CheckOut);

        if (overlap)
        {
            return Result<GetBookingDto>.Conflict("The selected dates overlap with one of your existing bookings.");
        }

        var hotel = await hotelsRepository.GetByIdAsync(hotelId);

        if (hotel == null)
        {
            return Result<GetBookingDto>.NotFound($"Hotel with id {hotelId} was not found.");
        }

        var nights = createDto.CheckOut.DayNumber - createDto.CheckIn.DayNumber;
        var totalPrice = hotel.PerNightRate * nights;

        var booking = new Booking
        {
            HotelId = createDto.HotelId,
            UserId = userId,
            CheckIn = createDto.CheckIn,
            CheckOut = createDto.CheckOut,
            Guests = createDto.Guests,
            TotalPrice = totalPrice
        };

        await repository.AddAsync(booking);

        booking = await repository.GetBookingWithHotelAsync(booking.Id);

        var response = new GetBookingDto(
            booking!.Id,
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
        var userId = GetUserId();

        var overlap = await repository.IsOverlapAsync(hotelId, userId, updateDto.CheckIn, updateDto.CheckOut, bookingId);

        if (overlap)
        {
            return Result<GetBookingDto>.Conflict("The updated dates overlap with one of your existing bookings.");
        }

        var booking = await repository.GetUserBookingForHotelAsync(bookingId, hotelId, userId);

        if (booking == null)
        {
            return Result<GetBookingDto>.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Conflict($"Booking with id {bookingId} is cancelled and cannot be updated.");
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
        var booking = await repository.GetBookingWithHotelAsync(bookingId, hotelId);

        if (booking == null)
        {
            return Result.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found.");
        }

        await repository.DeleteAsync(bookingId);

        return Result.Success();
    }

    private string GetUserId()
    {
        return httpContext?.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContext?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }
}