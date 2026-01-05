using System.Security.Claims;

namespace FestiveGuest.Core.Services;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(string userId, string email, string role);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    Task<bool> IsTokenValidAsync(string token);
}