using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Country;

public class CreateCountyDto
{
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    [Required]
    [MaxLength(3)]
    public required string ShortName { get; set; }
}
