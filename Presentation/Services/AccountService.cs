using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Presentation.Data;
using Presentation.Models;

namespace Presentation.Services;

public class AccountService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto userDto)
    {
        ArgumentNullException.ThrowIfNull(userDto);
        
        if( await ExistsAsync(userDto.Email))
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new AppUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName
        };
         
        return await _userManager.CreateAsync(user, userDto.Password);
    }

    public async Task<AppUser?> SignInAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null && await _userManager.CheckPasswordAsync(user, password))
        {
            return user;
        }
        return null;
    }


    public string GenerateJwtToken(AppUser user, IConfiguration configuration)
    {
        var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, $"{user.FirstName} {user.LastName}"),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }
}
