using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Country;

public class UpdateCountryDto : CreateCountyDto
{
    [Required]
    public int Id { get; set; }
}
