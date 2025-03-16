using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HoTeach
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            try
            {
                var principal = AuthHelper.ValidateToken(req);
                if (!AuthHelper.HasScope(principal, "hoteach:default"))
                {
                    return new UnauthorizedResult();
                }
                _logger.LogInformation($"Authenticated user: {principal.Identity.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return new UnauthorizedResult();
            }

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
