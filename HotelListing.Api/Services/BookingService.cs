using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
using HotelListing.Api.Models.Pagination;
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
    public async Task<Result<PagedResult<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId, PaginationParameters parameters)
    {
        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<PagedResult<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
        }

        var userId = GetUserId();

        var bookings = await repository.GetUserBookingsForHotelAsync(hotelId, userId, parameters);

        var dto = bookings.Data.Select(b => new GetBookingDto(
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

        var response = new PagedResult<GetBookingDto>
        {
            Data = dto,
            Metadata = bookings.Metadata
        };

        return Result<PagedResult<GetBookingDto>>.Success(response);
    }

    public async Task<Result<PagedResult<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters parameters)
    {
        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<PagedResult<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
        }

        var bookings = await repository.GetBookingsForHotelAsync(hotelId, parameters);

        var dto = bookings.Data.Select(b => new GetBookingDto(
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

        var response = new PagedResult<GetBookingDto>
        {
            Data = dto,
            Metadata = bookings.Metadata
        };

        return Result<PagedResult<GetBookingDto>>.Success(response);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(int hotelId, CreateBookingDto createDto)
    {
        if (hotelId != createDto.HotelId)
        {
            return Result<GetBookingDto>.BadRequest($"Route HotelId {hotelId} does not match Body HotelId {createDto.HotelId}.");
        }

        var userId = GetUserId();

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

        var response = new GetBookingDto(
            booking.Id,
            hotel.Id,
            hotel.Name,
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

    public async Task<Result<GetBookingDetailsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateDto)
    {
        var userId = GetUserId();

        var overlap = await repository.IsOverlapAsync(hotelId, userId, updateDto.CheckIn, updateDto.CheckOut, bookingId);

        if (overlap)
        {
            return Result<GetBookingDetailsDto>.Conflict("The updated dates overlap with one of your existing bookings.");
        }

        var booking = await repository.GetUserBookingForHotelTrackedAsync(bookingId, hotelId, userId);

        if (booking == null)
        {
            return Result<GetBookingDetailsDto>.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDetailsDto>.Conflict($"Booking with id {bookingId} is cancelled and cannot be updated.");
        }

        var hotel = await hotelsRepository.GetHotelWithCountryAsync(hotelId);

        if (hotel == null)
        {
            return Result<GetBookingDetailsDto>.NotFound($"Hotel with id {hotelId} not found.");
        }

        var perNightRate = hotel.PerNightRate;
        var nights = updateDto.CheckOut.DayNumber - updateDto.CheckIn.DayNumber;
        var totalPrice = perNightRate * nights;

        booking.TotalPrice = totalPrice;
        booking.CheckIn = updateDto.CheckIn;
        booking.CheckOut = updateDto.CheckOut;
        booking.Guests = updateDto.Guests;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(booking);

        var response = new GetBookingDetailsDto(
            booking.Id,
            booking.HotelId,
            hotel.Name,
            hotel.Address ?? "Unknown",
            hotel.Rating,
            hotel.Country!.Name!,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );

        return Result<GetBookingDetailsDto>.Success(response);
    }

    public async Task<Result> DeleteBookingAsync(int hotelId, int bookingId)
    {
        var booking = await repository.GetBookingForHotelAsync(bookingId, hotelId);

        if (booking == null)
        {
            return Result.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found.");
        }

        await repository.DeleteAsync(bookingId);

        return Result.Success();
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = GetUserId();

        var booking = await repository.GetUserBookingForHotelTrackedAsync(bookingId, hotelId, userId);

        if (booking == null)
        {
            return Result.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Conflict($"Booking with id {bookingId} is already cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(booking);

        return Result.Success();
    }

    public async Task<Result> AdminUpdateBookingStatusAsync(int hotelId, int bookingId, BookingStatus bookingStatus)
    {
        var booking = await repository.GetBookingForHotelAsync(bookingId, hotelId);

        if (booking == null)
        {
            return Result.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Conflict($"Booking with id {bookingId} is already cancelled.");
        }

        if (booking.Status == bookingStatus)
        {
            return Result.Success();
        }

        booking.Status = bookingStatus;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpdateAsync(booking);

        return Result.Success();
    }

    public async Task<Result<GetBookingDetailsDto>> GetUserBookingDetailsAsync(int hotelId, int bookingId)
    {
        var userId = GetUserId();

        var booking = await repository.GetBookingWithHotelAndCountryAsync(bookingId, userId);

        if (booking == null || booking.HotelId != hotelId)
        {
            return Result<GetBookingDetailsDto>.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        var response = new GetBookingDetailsDto(
            booking.Id,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.Hotel!.Address ?? "Unknown",
            booking.Hotel!.Rating,
            booking.Hotel?.Country?.Name ?? "Unknown",
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );

        return Result<GetBookingDetailsDto>.Success(response);
    }

    public async Task<Result<GetAdminBookingDetailsDto>> GetAdminBookingDetailsAsync(int hotelId, int bookingId)
    {
        var booking = await repository.GetBookingWithHotelAndUserAsync(bookingId, hotelId);

        if (booking == null || booking.HotelId != hotelId)
        {
            return Result<GetAdminBookingDetailsDto>.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        var response = new GetAdminBookingDetailsDto(
            booking.Id,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.User!.FullName,
            booking.User!.Email ?? "Unknown",
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.Status.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
            );

        return Result<GetAdminBookingDetailsDto>.Success(response);
    }
    private string GetUserId()
    {
        return httpContext?.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContext?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }
}