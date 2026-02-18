using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.DTOs.Booking;

public record UpdateBookingDto : IValidatableObject
{
    public DateOnly CheckIn { get; init; }
    public DateOnly CheckOut { get; init; }

    [Range(1, 10)]
    public int Guests { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOut <= CheckIn)
        {
            yield return new ValidationResult(
                "Check-out must be after check-in.",
                [nameof(CheckOut), nameof(CheckIn)]
            );
        }
    }
}
    