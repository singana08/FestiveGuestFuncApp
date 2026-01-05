using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FestiveGuest.Core.Services;

public class JwtService : IJwtService
{
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<JwtService> _logger;
    private string? _cachedSecret;
    private DateTime _secretCacheTime = DateTime.MinValue;
    private readonly TimeSpan _secretCacheExpiry = TimeSpan.FromHours(1);

    public JwtService(IKeyVaultService keyVaultService, ILogger<JwtService> logger)
    {
        _keyVaultService = keyVaultService;
        _logger = logger;
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
            expires: DateTime.UtcNow.AddHours(8), // Reduced from 24 to 8 hours
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
                ClockSkew = TimeSpan.FromMinutes(5), // Allow 5 minutes clock skew
                RequireExpirationTime = true
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Additional validation for JWT
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid JWT algorithm or token type");
                return null;
            }

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token expired: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Invalid token signature: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
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
        // Check if cached secret is still valid
        if (_cachedSecret != null && DateTime.UtcNow - _secretCacheTime < _secretCacheExpiry)
        {
            return _cachedSecret;
        }

        try
        {
            _cachedSecret = await _keyVaultService.GetJwtSecretAsync();
            _secretCacheTime = DateTime.UtcNow;
            
            if (string.IsNullOrEmpty(_cachedSecret))
            {
                throw new InvalidOperationException("JWT secret is null or empty");
            }
            
            return _cachedSecret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve JWT secret from Key Vault");
            throw;
        }
    }
}