using FestiveGuest.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class RefreshTokenFunction
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenFunction> _logger;

    public RefreshTokenFunction(IJwtService jwtService, ILogger<RefreshTokenFunction> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [Function("RefreshToken")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options")] HttpRequestData req)
    {
        // Handle CORS preflight
        if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var corsResponse = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(corsResponse);
            return corsResponse;
        }

        try
        {
            // Validate existing JWT token
            var authHeader = req.Headers.GetValues("Authorization").FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCorsHeaders(unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new { error = "Authorization token required" });
                return unauthorizedResponse;
            }

            var token = authHeader["Bearer ".Length..];
            var principal = await _jwtService.ValidateTokenAsync(token);
            
            if (principal == null)
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCorsHeaders(unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return unauthorizedResponse;
            }

            // Extract user info from existing token
            var userId = principal.FindFirst("userId")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var role = principal.FindFirst("role")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCorsHeaders(badRequestResponse);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Invalid token claims" });
                return badRequestResponse;
            }

            // Generate new token
            var newToken = await _jwtService.GenerateTokenAsync(userId, email, role);

            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response);
            await response.WriteAsJsonAsync(new { token = newToken });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RefreshToken function");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            AddCorsHeaders(errorResponse);
            await errorResponse.WriteAsJsonAsync(new { error = "Token refresh service temporarily unavailable" });
            return errorResponse;
        }
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}