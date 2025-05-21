using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Presentation.Data;
using Presentation.Models;

namespace Presentation.Services;

public class AccountService(UserManager<AppUser> userManager, IMemoryCache memoryCache, ServiceBusSender serviceBusSender)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ServiceBusSender _serviceBusSender = serviceBusSender;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto userDto)
    {
        ArgumentNullException.ThrowIfNull(userDto);

        var user = new AppUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName
        };

        var response = await _userManager.CreateAsync(user, userDto.Password);
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        return response;

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

    public async Task SendVerificationCodeAsync(string email)
    {
        var code = new Random().Next(100000, 999999).ToString();
        _memoryCache.Set(email, code, TimeSpan.FromMinutes(10));

        var message = new
        {
            Email = email,
            Code = code
        };
        var json = JsonSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(json);
        await _serviceBusSender.SendMessageAsync(serviceBusMessage);
    }

    public bool VerifyCode(UserVerificationDto dto)
    {
        if (_memoryCache.TryGetValue(dto.Email, out string? code) && code == dto.Code)
        {
            _memoryCache.Remove(dto.Email);
            return true;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }
}
