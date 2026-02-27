using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListing.Api.Services;

public class BookingService(
    IBookingsRepository repository,
    IHotelsRepository hotelsRepository,
    IHttpContextAccessor httpContext,
    IMapper mapper
    ) : IBookingsService
{
    public async Task<Result<PagedResult<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId, PaginationParameters paging, BookingFilterParameters filters)
    {
        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<PagedResult<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
        }

        var query = repository.GetAllAsQueryable();

        query = ApplyFilters(hotelId, filters, query);

        var userId = GetUserId();

        var pagedBookings = await repository.GetUserBookingsForHotelAsync(userId, paging, query);

        var bookings = mapper.Map<IEnumerable<GetBookingDto>>(pagedBookings.Data);

        var response = new PagedResult<GetBookingDto>
        {
            Data = bookings,
            Metadata = pagedBookings.Metadata
        };

        return Result<PagedResult<GetBookingDto>>.Success(response);
    }

    public async Task<Result<PagedResult<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters paging, BookingFilterParameters filters)
    {
        var hotelExist = await hotelsRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExist)
        {
            return Result<PagedResult<GetBookingDto>>.NotFound($"Hotel with id {hotelId} does not exist.");
        }

        var query = repository.GetAllAsQueryable();

        query = ApplyFilters(hotelId, filters, query);

        var pagedBookings = await repository.GetBookingsForHotelAsync(paging, query);

        var bookings = mapper.Map<IEnumerable<GetBookingDto>>(pagedBookings.Data);

        var response = new PagedResult<GetBookingDto>
        {
            Data = bookings,
            Metadata = pagedBookings.Metadata
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

        var booking = mapper.Map<Booking>(createDto);

        booking.UserId = userId;
        booking.TotalPrice = totalPrice;

        await repository.AddAsync(booking);

        var response = mapper.Map<GetBookingDto>(booking);

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

        booking.Hotel = hotel;
        var response = mapper.Map<GetBookingDetailsDto>(booking);

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

        var response = mapper.Map<GetBookingDetailsDto>(booking);

        return Result<GetBookingDetailsDto>.Success(response);
    }

    public async Task<Result<GetAdminBookingDetailsDto>> GetAdminBookingDetailsAsync(int hotelId, int bookingId)
    {
        var booking = await repository.GetBookingWithHotelAndUserAsync(bookingId, hotelId);

        if (booking == null || booking.HotelId != hotelId)
        {
            return Result<GetAdminBookingDetailsDto>.NotFound($"Booking with id {bookingId} for hotel {hotelId} was not found or access is denied.");
        }

        var response = mapper.Map<GetAdminBookingDetailsDto>(booking);

        return Result<GetAdminBookingDetailsDto>.Success(response);
    }
    private string GetUserId()
    {
        return httpContext?.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContext?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }

    private IQueryable<Booking> ApplyFilters(int hotelId, BookingFilterParameters filters, IQueryable<Booking> query)
    {
        query = query.Where(b => b.HotelId == hotelId);

        if (filters.Status.HasValue)
        {
            query = query.Where(b => b.Status == filters.Status.Value);
        }

        if (filters.CheckInFrom.HasValue)
        {
            query = query.Where(b => b.CheckIn >= filters.CheckInFrom.Value);
        }

        if (filters.CheckInTo.HasValue)
        {
            query = query.Where(b => b.CheckIn <= filters.CheckInTo.Value);
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(b => b.TotalPrice <= filters.MaxPrice.Value);
        }

        if (filters.MinGuests.HasValue)
        {
            query = query.Where(b => b.Guests >= filters.MinGuests.Value);
        }

        if (filters.MaxGuests.HasValue)
        {
            query = query.Where(b => b.Guests <= filters.MaxGuests.Value);
        }

        query = filters.SortBy?.ToLower() switch
        {
            "checkin" => filters.SortDescending ?
                query.OrderByDescending(b => b.CheckIn) : query.OrderBy(b => b.CheckIn),
            "checkout" => filters.SortDescending ?
                query.OrderByDescending(b => b.CheckOut) : query.OrderBy(b => b.CheckOut),
            "price" => filters.SortDescending ?
                query.OrderByDescending(b => b.TotalPrice) : query.OrderBy(b => b.TotalPrice),
            "created" => filters.SortDescending ?
                query.OrderByDescending(b => b.CreatedAtUtc) : query.OrderBy(b => b.CreatedAtUtc),
            _ => query.OrderByDescending(b => b.CheckIn).ThenByDescending(b => b.Id)
        };

        return query;
    }
}