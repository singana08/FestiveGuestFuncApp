using FestiveGuest.Core.Services;
using FestiveGuest.Models.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace FestiveGuest.Functions;

public class CreateUpdateUserFunction
{
    private readonly IUserService _userService;
    private readonly ILogger<CreateUpdateUserFunction> _logger;

    public CreateUpdateUserFunction(IUserService userService, ILogger<CreateUpdateUserFunction> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [Function("CreateUpdateUser")]
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
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createUserRequest = JsonSerializer.Deserialize<CreateUserRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (createUserRequest == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCorsHeaders(badRequestResponse);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badRequestResponse;
            }

            var result = await _userService.CreateOrUpdateUserAsync(createUserRequest);

            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
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
            _logger.LogError(ex, "Error in CreateUpdateUser function");
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            AddCorsHeaders(errorResponse);
            await errorResponse.WriteAsJsonAsync(new { error = "Registration service temporarily unavailable" });
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