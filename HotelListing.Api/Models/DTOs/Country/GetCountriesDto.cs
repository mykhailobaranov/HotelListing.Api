namespace HotelListing.Api.Models.DTOs.Country;

public record GetCountriesDto(
    int CountryId,
    string Name,
    string ShortName
    );
