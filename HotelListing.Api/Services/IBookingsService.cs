using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.Enums;

namespace HotelListing.Api.Services;

public interface IBookingsService
{
    Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId);
    //Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsAsync();
    Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId);
    Task<Result<GetBookingDetailsDto>> GetUserBookingDetailsAsync(int hotelId, int bookingId);
    Task<Result<GetAdminBookingDetailsDto>> GetAdminBookingDetailsAsync(int hotelId, int bookingId);
    Task<Result<GetBookingDto>> CreateBookingAsync(int hotelId, CreateBookingDto createDto);
    Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateDto);
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminUpdateBookingStatusAsync(int hotelId, int bookingId, BookingStatus bookingStatus);
    Task<Result> DeleteBookingAsync(int hotelId, int bookingId);
}