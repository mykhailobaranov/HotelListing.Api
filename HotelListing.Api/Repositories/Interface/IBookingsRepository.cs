using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;

public interface IBookingsRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetBookingWithHotelAndCountryAsync(int bookingId, string userId);
    Task<Booking?> GetBookingWithHotelAndUserAsync(int bookingId, int hotelId);
    Task<Booking?> GetBookingForHotelAsync(int bookingId, int hotelId);
    Task<IEnumerable<Booking>> GetBookingsForHotelAsync(int hotelId);
    Task<Booking?> GetUserBookingAsync(int bookingId, string userId);
    Task<Booking?> GetUserBookingForHotelAsync(int bookingId, int hotelId, string userId);
    Task<Booking?> GetUserBookingForHotelTrackedAsync(int bookingId, int hotelId, string userId);
    Task<IEnumerable<Booking>> GetUserBookingsForHotelAsync(int hotelId, string userId);
    Task<bool> IsOverlapAsync(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null);
}