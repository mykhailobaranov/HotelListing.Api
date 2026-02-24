using HotelListing.Api.Data;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Api.Services;

public class UsersService(
    HotelListingDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<JwtSettings> jwtOptions,
    IConfiguration configuration) : IUsersService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var isRoleExist = await roleManager.RoleExistsAsync(registerUserDto.Role);
        if (!isRoleExist)
        {
            return Result<RegisteredUserDto>.BadRequest($"Role '{registerUserDto.Role}' doesn't exist.");
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

        if (registerUserDto.Role == "Hotel Admin")
        {
            var hotelAdmin = db.HotelAdmins.Add(
                new HotelAdmin
                {
                    UserId = user.Id,
                    HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault()
                });
            await db.SaveChangesAsync();
        }

        //mapping to be added
        var response = new RegisteredUserDto
        {
            Id = user.Id,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Role = registerUserDto.Role,
        };

        return Result<RegisteredUserDto>.Success(response);
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
        // Set basic user claims
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Name, user.FullName)
        };

        // Set user role claims
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims.AddRange(roleClaims);

        if (roles.Contains("Hotel Admin"))
        {
            var managedHotelId = await db.HotelAdmins
                .Where(ha => ha.UserId == user.Id)
                .Select(ha => ha.HotelId)
                .FirstOrDefaultAsync();

            if (managedHotelId != 0)
            {
                claims.Add(new Claim("HotelId", managedHotelId.ToString()));
            }
        }

        // Set JWT Key credentials
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create an encoded token
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtOptions.Value.DurationInMinutes)),
            signingCredentials: credentials
            );

        // Return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}