using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.Api.Services;

public class UsersService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : IUsersService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var isRoleExist = await roleManager.RoleExistsAsync(registerUserDto.Role);
        if (!isRoleExist)
        {
            return Result<RegisteredUserDto>.BadRequest($"Role '{registerUserDto.Role}' does not exist.");
        }

        //mapping to be added
        var user = new ApplicationUser
        {
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            UserName = registerUserDto.Email
        };

        var result = await userManager.CreateAsync(user, registerUserDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<RegisteredUserDto>.BadRequest(errors);
        }

        await userManager.AddToRoleAsync(user, registerUserDto.Role);

        //mapping to be added
        var registeredUser = new RegisteredUserDto
        {
            Id = user.Id,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Role = registerUserDto.Role,
        };

        return Result<RegisteredUserDto>.Success(registeredUser);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        var user = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (user == null)
        {
            return Result<string>.BadRequest("Wrong email or password.");
        }

        var valid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
        if (!valid)
        {
            return Result<string>.BadRequest("Wrong email or password.");
        }

        var token = await GenerateToken(user);

        return Result<string>.Success(token);
    }

    private async Task<string> GenerateToken(ApplicationUser user)
    {
        //JWT Token generation to be added
        return "token";
    }
}