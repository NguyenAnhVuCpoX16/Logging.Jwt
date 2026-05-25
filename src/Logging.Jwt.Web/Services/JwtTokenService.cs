using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Logging.Jwt.Data.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Logging.Jwt.Web.Services;

public class JwtTokenService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim("display_name", user.DisplayName));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
