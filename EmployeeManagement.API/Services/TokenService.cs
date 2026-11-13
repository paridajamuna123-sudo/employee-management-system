using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Services;

public interface ITokenService
{
    (string Token, DateTime Expiration) GenerateJwtToken(ApplicationUser user, IList<string> roles);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime Expiration) GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "SuperSecretKeyForEMSDevelopmentSecurityAuthentication2026!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "EmployeeManagement.API";
        var audience = _configuration["JwtSettings:Audience"] ?? "EmployeeManagement.UI";
        var expirationMinutes = int.TryParse(_configuration["JwtSettings:ExpirationInMinutes"], out var mins) ? mins : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName ?? user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }
}
