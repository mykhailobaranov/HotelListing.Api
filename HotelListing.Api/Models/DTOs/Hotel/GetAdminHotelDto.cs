using HotelListing.Api.Models.Domain;

namespace HotelListing.Api.Models.DTOs.Hotel;

public record GetAdminHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    decimal PerNightRate,
    string Country,
    ICollection<GetHotelAdminDto> Admins
);