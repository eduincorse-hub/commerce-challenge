using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommerceApi.Data;
using CommerceApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace CommerceApi.Services;

/// <summary>Inicio de sesión y generación de tokens JWT.</summary>
public interface IAuthService
{
    /// <summary>Valida las credenciales. Devuelve null si son incorrectas.</summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>Crea el usuario inicial (SeedUser) si no existe ningún usuario.</summary>
    Task EnsureDefaultUserAsync();
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var expires = DateTime.UtcNow.AddMinutes(_config.GetValue("Jwt:ExpireMinutes", 60));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, user.Username) },
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.Username);
    }

    public async Task EnsureDefaultUserAsync()
    {
        if (await _users.AnyAsync()) return;

        var username = _config["SeedUser:Username"] ?? "admin";
        var password = _config["SeedUser:Password"] ?? "Admin123*";
        await _users.CreateAsync(username, BCrypt.Net.BCrypt.HashPassword(password));
    }
}