using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Auth;

namespace HotelListing.Api.Services;

public interface IUsersService
{
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto dto);
    Task<Result<string>> LoginAsync(LoginUserDto dto);
}