using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;
using HotelListing.Api.Models.Pagination;

namespace HotelListing.Api.Services;

public interface IBookingsService
{
    Task<Result<PagedResult<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId, PaginationParameters parameters);
    //Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsAsync();
    Task<Result<PagedResult<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters parameters);
    Task<Result<GetBookingDetailsDto>> GetUserBookingDetailsAsync(int hotelId, int bookingId);
    Task<Result<GetAdminBookingDetailsDto>> GetAdminBookingDetailsAsync(int hotelId, int bookingId);
    Task<Result<GetBookingDto>> CreateBookingAsync(int hotelId, CreateBookingDto createDto);
    Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateDto);
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminUpdateBookingStatusAsync(int hotelId, int bookingId, BookingStatus bookingStatus);
    Task<Result> DeleteBookingAsync(int hotelId, int bookingId);
}