using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class LocationFunctions
{
    private readonly ILogger<LocationFunctions> _logger;

    public LocationFunctions(ILogger<LocationFunctions> logger)
    {
        _logger = logger;
    }

    [Function("GetLocations")]
    public async Task<HttpResponseData> GetLocations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options")] HttpRequestData req)
    {
        if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var corsResponse = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(corsResponse);
            return corsResponse;
        }

        var locations = new[]
        {
            new { id = 1, name = "Mumbai", state = "Maharashtra" },
            new { id = 2, name = "Delhi", state = "Delhi" },
            new { id = 3, name = "Bangalore", state = "Karnataka" }
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(response);
        await response.WriteAsJsonAsync(new { locations });
        return response;
    }

    [Function("SeedLocations")]
    public async Task<HttpResponseData> SeedLocations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options")] HttpRequestData req)
    {
        if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var corsResponse = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(corsResponse);
            return corsResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(response);
        await response.WriteAsJsonAsync(new { message = "Locations seeded successfully" });
        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}