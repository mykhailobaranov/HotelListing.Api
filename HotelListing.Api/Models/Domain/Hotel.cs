namespace HotelListing.Api.Models.Domain;

public class Hotel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public double Rating { get; set; }
    public required decimal PerNightRate { get; set; }

    public int CountryId { get; set; }
    public Country? Country { get; set; }

    public ICollection<HotelAdmin> Admins { get; set; } = [];
}