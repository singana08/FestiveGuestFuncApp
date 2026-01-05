using FestiveGuest.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class GetUserFunction
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GetUserFunction> _logger;

    public GetUserFunction(IUserService userService, IJwtService jwtService, ILogger<GetUserFunction> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [Function("GetUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options")] HttpRequestData req)
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
            // Validate JWT token
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

            // Extract user info from token
            var userId = principal.FindFirst("userId")?.Value;
            var role = principal.FindFirst("role")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCorsHeaders(badRequestResponse);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Invalid token claims" });
                return badRequestResponse;
            }

            var result = await _userService.GetUserAsync(role, userId);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            AddCorsHeaders(response);

            if (result.Success)
            {
                await response.WriteAsJsonAsync(result.Data);
            }
            else
            {
                await response.WriteAsJsonAsync(new { error = result.Error });
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUser function");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            AddCorsHeaders(errorResponse);
            await errorResponse.WriteAsJsonAsync(new { error = "Service temporarily unavailable" });
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