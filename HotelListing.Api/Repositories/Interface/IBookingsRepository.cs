using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;

public interface IBookingsRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetBookingDetailsAsync(int id);
    Task<Booking?> GetBookingDetailsAsync(int id, int hotelId);
    Task<IEnumerable<Booking>> GetBookingsForHotelAsync(int id);
    Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null);
    Task<Booking?> GetUserBookingForHotelAsync(int bookingId, int hotelId, string userId);
}