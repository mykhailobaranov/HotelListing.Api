using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Auth;

public record RegisterUserDto : IValidatableObject
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; init; } = string.Empty;

    public string Role { get; init; } = RoleNames.User;

    public int? AssociatedHotelId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Role == RoleNames.HotelAdmin && AssociatedHotelId.GetValueOrDefault() < 1)
        {
            yield return new ValidationResult(
                "Please provide a valid Hotel Id",
                [nameof(AssociatedHotelId)]);
        }
    }
}
