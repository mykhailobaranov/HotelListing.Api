using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Auth;

public record LoginUserDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    [Required]
    public string Password { get; init; } = string.Empty;
}
