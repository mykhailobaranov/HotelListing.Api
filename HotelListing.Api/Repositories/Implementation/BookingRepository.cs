using HotelListing.Api.Data;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation;

public class BookingRepository : GenericRepository<Booking>, IBookingsRepository
{
    private readonly HotelListingDbContext _db;

    public BookingRepository(HotelListingDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Booking?> GetBookingDetailsAsync(int id)
    {
        return await _db.Bookings
                .Include(q => q.Hotel)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Booking?> GetBookingDetailsAsync(int id, int hotelId)
    {
        return await _db.Bookings
            .FirstOrDefaultAsync(b => b.HotelId == hotelId && b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetBookingsForHotelAsync(int id)
    {
        return await _db.Bookings
                .Include(q => q.Hotel)
                .AsNoTracking()
                .Where(q => q.HotelId == id)
                .OrderBy(q=>q.CheckIn)
                .ToListAsync(); 
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

    public async Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = _db.Bookings
            .Where(
                    b => b.HotelId == hotelId
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
