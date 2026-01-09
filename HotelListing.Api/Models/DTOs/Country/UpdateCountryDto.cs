using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Country;

public class UpdateCountryDto : CreateCountryDto
{
    [Required]
    public int CountryId { get; set; }
}
