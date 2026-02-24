using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Interface;

public interface IBookingsRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetBookingWithHotelAndCountryAsync(int bookingId, string userId);
    Task<Booking?> GetBookingWithHotelAndUserAsync(int bookingId, int hotelId);
    Task<Booking?> GetBookingForHotelAsync(int bookingId, int hotelId);
    Task<PagedResult<Booking>> GetBookingsForHotelAsync(int hotelId, PaginationParameters parameters);
    Task<Booking?> GetUserBookingAsync(int bookingId, string userId);
    Task<Booking?> GetUserBookingForHotelAsync(int bookingId, int hotelId, string userId);
    Task<Booking?> GetUserBookingForHotelTrackedAsync(int bookingId, int hotelId, string userId);
    Task<PagedResult<Booking>> GetUserBookingsForHotelAsync(int hotelId, string userId, PaginationParameters parameters);
    Task<bool> IsOverlapAsync(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null);
}