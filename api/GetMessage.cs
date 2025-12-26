using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api;

public class GetMessage
{
    private readonly ILogger<GetMessage> _logger;

    public GetMessage(ILogger<GetMessage> logger)
    {
        _logger = logger;
    }

    [Function("GetMessage")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Hello from Team Manager API!");
    }
}
