namespace HotelListing.Api.Models.DTOs.Hotel;
public record GetHotelDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Address { get; init; }
    public double Rating { get; init; }
    public string? Country { get; init; }
}

public record GetHotelSlimDto(
    int Id,
    string Name,
    string Address,
    double Rating
    );