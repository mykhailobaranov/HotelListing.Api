using HotelListing.Api.Models.DTOs.Hotel;

namespace HotelListing.Api.Models.DTOs.Country;

public record GetCountryDto(
    int CountryId,
    string Name,
    string ShortName,
    List<GetHotelSlimDto> Hotels
    );