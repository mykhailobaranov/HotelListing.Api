using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Hotel;

public record UpdateHotelDto : CreateHotelDto
{
    [Required]
    public int Id { get; set; }
}