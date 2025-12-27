namespace HotelListing.Api.Models.DTOs.Hotel;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    string Country);

public record GetHotelSlimDto(
    int Id,
    string Name,
    string Address,
    double Rating
    );
