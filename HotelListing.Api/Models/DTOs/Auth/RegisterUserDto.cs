using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Auth;

public record RegisterUserDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; init; } = string.Empty;

    public string Role { get; init; } = "User";
}
