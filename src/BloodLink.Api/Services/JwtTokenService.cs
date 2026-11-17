using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BloodLink.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BloodLink.Api.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "BloodLink";
    public string Audience { get; set; } = "BloodLink.Client";

    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 60 * 24;
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Donor donor);
}

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _time;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider time)
    {
        _options = options.Value;
        _time = time;
        if (Encoding.UTF8.GetByteCount(_options.Key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key має містити щонайменше 32 байти.");
        }
    }

    public (string Token, DateTime ExpiresAt) CreateToken(Donor donor)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_options.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, donor.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, donor.Email),
            new Claim(JwtRegisteredClaimNames.Name, donor.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
