using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class ChatFunctions
{
    private readonly ILogger<ChatFunctions> _logger;

    public ChatFunctions(ILogger<ChatFunctions> logger)
    {
        _logger = logger;
    }

    [Function("IssueChatToken")]
    public async Task<HttpResponseData> IssueChatToken(
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
        await response.WriteAsJsonAsync(new { chatToken = "sample-chat-token" });
        return response;
    }

    [Function("GetOrCreateChatThread")]
    public async Task<HttpResponseData> GetOrCreateChatThread(
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
        await response.WriteAsJsonAsync(new { threadId = "sample-thread-id" });
        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}