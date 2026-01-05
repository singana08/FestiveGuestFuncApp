using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FestiveGuest.Functions;

public class PaymentFunctions
{
    private readonly ILogger<PaymentFunctions> _logger;

    public PaymentFunctions(ILogger<PaymentFunctions> logger)
    {
        _logger = logger;
    }

    [Function("RecordPayment")]
    public async Task<HttpResponseData> RecordPayment(
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
        await response.WriteAsJsonAsync(new { message = "Payment recorded successfully" });
        return response;
    }

    [Function("VerifyPayment")]
    public async Task<HttpResponseData> VerifyPayment(
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
        await response.WriteAsJsonAsync(new { verified = true });
        return response;
    }

    [Function("ListPayments")]
    public async Task<HttpResponseData> ListPayments(
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
        await response.WriteAsJsonAsync(new { payments = new object[] { } });
        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}