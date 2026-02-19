namespace HotelListing.Api.Models;

public enum ErrorType
{
    None = 0,
    BadRequest = 400,
    NotFound = 404,
    Conflict = 409,
    Failure = 500
}