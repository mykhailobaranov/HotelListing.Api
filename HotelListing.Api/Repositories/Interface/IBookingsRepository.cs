using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;

public interface IBookingsRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetBookingWithHotelAsync(int id);
    Task<Booking?> GetBookingWithHotelAsync(int id, int hotelId);
    Task<IEnumerable<Booking>> GetBookingsForHotelAsync(int id);
    Task<Booking?> GetUserBookingAsync(int bookingId, string userId);
    Task<Booking?> GetUserBookingForHotelAsync(int bookingId, int hotelId, string userId);
    Task<IEnumerable<Booking>> GetUserBookingsForHotelAsync(int hotelId, string userId);
    Task<bool> IsOverlapAsync(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null);
}