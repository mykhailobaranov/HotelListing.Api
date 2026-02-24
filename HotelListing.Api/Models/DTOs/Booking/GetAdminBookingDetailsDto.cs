namespace HotelListing.Api.Models.DTOs.Booking;

public record GetAdminBookingDetailsDto(
    int Id,
    int HotelId,
    string HotelName,
    string GuestFullName,
    string GuestEmail,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
    );