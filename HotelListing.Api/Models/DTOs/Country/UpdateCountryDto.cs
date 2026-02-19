using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Country;

public record UpdateCountryDto : CreateCountryDto
{
    [Required]
    public int CountryId { get; set; }
}