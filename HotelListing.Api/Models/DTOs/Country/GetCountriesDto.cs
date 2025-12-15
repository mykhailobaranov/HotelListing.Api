namespace HotelListing.Api.Models.DTOs.Country;

public record GetCountriesDto(
    int Id,
    string Name,
    string ShortName
    );
