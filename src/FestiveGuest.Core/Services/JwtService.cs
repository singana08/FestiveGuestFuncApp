using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FestiveGuest.Core.Services;

public class JwtService : IJwtService
{
    private readonly IKeyVaultService _keyVaultService;
    private string? _cachedSecret;

    public JwtService(IKeyVaultService keyVaultService)
    {
        _keyVaultService = keyVaultService;
    }

    public async Task<string> GenerateTokenAsync(string userId, string email, string role)
    {
        var secret = await GetJwtSecretAsync();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId),
            new Claim("email", email),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: "FestiveGuest",
            audience: "FestiveGuest",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var secret = await GetJwtSecretAsync();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = "FestiveGuest",
                ValidateAudience = true,
                ValidAudience = "FestiveGuest",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        var principal = await ValidateTokenAsync(token);
        return principal != null;
    }

    private async Task<string> GetJwtSecretAsync()
    {
        _cachedSecret ??= await _keyVaultService.GetJwtSecretAsync();
        return _cachedSecret;
    }
}