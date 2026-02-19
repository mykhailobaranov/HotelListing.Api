using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Hotel;

public record CreateHotelDto
{
    [Required]
    public required string Name { get; init; }
    [Required]
    [MaxLength(150)]
    public required string Address { get; init; }
    [Range(0, 5)]
    public double Rating { get; init; }
    [Required]
    [Range(0, 100000)]
    public decimal PerNightRate { get; init; }
    [Required]
    public int CountryId { get; init; }
}