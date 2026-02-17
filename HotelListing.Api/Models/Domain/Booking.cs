namespace HotelListing.Api.Models.Domain;

public class Booking
{
    public int Id { get; set; }

    public required int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public required string UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public required DateOnly CheckIn { get; set; }
    public required DateOnly CheckOut { get; set; }
    public required int Guests { get; set; }

    public required decimal TotalPrice { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
