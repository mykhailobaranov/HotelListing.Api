using HotelListing.Api.Data;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation;

public class BookingRepository : GenericRepository<Booking>, IBookingsRepository
{
    private readonly HotelListingDbContext _db;

    public BookingRepository(HotelListingDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Booking?> GetBookingWithHotelAsync(int id)
    {
        return await _db.Bookings
                .Include(b => b.Hotel)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetBookingWithHotelAsync(int id, int hotelId)
    {
        return await _db.Bookings
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.HotelId == hotelId && b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetBookingsForHotelAsync(int id)
    {
        return await _db.Bookings
                .Include(b => b.Hotel)
                .AsNoTracking()
                .Where(b => b.HotelId == id)
                .OrderBy(b => b.CheckIn)
                .ToListAsync();
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
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId);
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsForHotelAsync(int hotelId, string userId)
    {
        return await _db.Bookings
            .Include(b => b.Hotel)
            .AsNoTracking()
            .Where(b => b.HotelId == hotelId
                     && b.UserId == userId)
            .ToListAsync();
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