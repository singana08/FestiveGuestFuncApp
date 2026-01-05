using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class FileFunctions
{
    private readonly ILogger<FileFunctions> _logger;

    public FileFunctions(ILogger<FileFunctions> logger)
    {
        _logger = logger;
    }

    [Function("UploadImage")]
    public async Task<HttpResponseData> UploadImage(
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
        await response.WriteAsJsonAsync(new { imageUrl = "https://example.com/uploaded-image.jpg" });
        return response;
    }

    [Function("GetImageUrl")]
    public async Task<HttpResponseData> GetImageUrl(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options")] HttpRequestData req)
    {
        if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var corsResponse = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(corsResponse);
            return corsResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(response);
        await response.WriteAsJsonAsync(new { imageUrl = "https://example.com/image.jpg" });
        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}