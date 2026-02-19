using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Country;

public record CreateCountryDto
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; init; }
    [Required]
    [MaxLength(3)]
    public required string ShortName { get; init; }
}