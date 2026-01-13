namespace HotelListing.Api.Models;

public enum ErrorType
{
    None = 0,
    NotFound = 404,
    BadRequest = 400,
    Conflict = 409,
    Failure = 500
}
