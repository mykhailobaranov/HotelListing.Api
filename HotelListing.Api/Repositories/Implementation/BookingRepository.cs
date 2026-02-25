using AutoMapper;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation;

public class BookingRepository : GenericRepository<Booking>, IBookingsRepository
{
    private readonly HotelListingDbContext _db;

    public BookingRepository(HotelListingDbContext db, IMapper mapper) : base(db, mapper)
    {
        _db = db;
    }

    public async Task<Booking?> GetBookingWithHotelAndCountryAsync(int bookingId, string userId)
    {
        return await _db.Bookings
                .Include(b => b.Hotel)
                    .ThenInclude(h => h.Country)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
    }

    public async Task<Booking?> GetBookingWithHotelAndUserAsync(int bookingId, int hotelId)
    {
        return await _db.Bookings
               .Include(b => b.Hotel)
               .Include(b => b.User)
               .AsNoTracking()
               .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);
    }

    public async Task<Booking?> GetBookingForHotelAsync(int bookingId, int hotelId)
    {
        return await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);
    }

    public async Task<PagedResult<Booking>> GetBookingsForHotelAsync(PaginationParameters parameters, IQueryable<Booking> query)
    {
         return await query
                .Include(b => b.Hotel)
                .ToPagedResultAsync(parameters);
    }

    public async Task<Booking?> GetUserBookingAsync(int bookingId, string userId)
    {
        return await _db.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.UserId == userId);
    }

    public async Task<Booking?> GetUserBookingForHotelAsync(int bookingId, int hotelId, string userId)
    {
        return await _db.Bookings
            .Include(b => b.Hotel)
            .AsNoTracking()
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId);
    }

    public async Task<Booking?> GetUserBookingForHotelTrackedAsync(int bookingId, int hotelId, string userId)
    {
        return await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId);
    }

    public async Task<PagedResult<Booking>> GetUserBookingsForHotelAsync(
        string userId, PaginationParameters parameters, IQueryable<Booking> query)
    {
        return await query
            .Include(b => b.Hotel)
            .Where(b =>  b.UserId == userId)
            .ToPagedResultAsync(parameters);
    }

    public async Task<bool> IsOverlapAsync(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = _db.Bookings
            .Where(b => b.HotelId == hotelId
                     && b.Status != BookingStatus.Cancelled
                     && checkIn < b.CheckOut
                     && checkOut > b.CheckIn
                     && b.UserId == userId)
            .AsQueryable();

        if (bookingId.HasValue)
        {
            query = query.Where(q => q.Id != bookingId.Value);
        }

        return await query.AnyAsync();
    }
}