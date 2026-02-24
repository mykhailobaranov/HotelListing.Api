namespace HotelListing.Api.Models.DTOs.Booking;

public record GetBookingDetailsDto(
    int Id,
    int HotelId,
    string HotelName,
    string HotelAddress,
    double HotelRating,
    string HotelCountry,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
    );