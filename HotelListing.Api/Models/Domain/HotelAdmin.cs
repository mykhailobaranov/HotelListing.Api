namespace HotelListing.Api.Models.Domain;

public class HotelAdmin
{
    public int Id { get; set; }

    public required int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public required string UserId { get; set; }
    public ApplicationUser? User { get; set; }
}