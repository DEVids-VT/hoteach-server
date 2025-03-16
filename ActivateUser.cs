using Auth0.AuthenticationApi.Models;
using Auth0.AuthenticationApi;
using HoTeach.Entities;
using HoTeach.Infrastructure.Interfaces;
using HoTeach.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi;

namespace HoTeach
{
    public class ActivateUser
    {
        private readonly ILogger<ActivateUser> logger;
        private readonly IRepository<Payment> paymentRepository;

        public ActivateUser(ILogger<ActivateUser> logger, IRepository<Payment> paymentRepository)
        {
            this.logger = logger;
            this.paymentRepository = paymentRepository;
        }

        [Function("ActivateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            try
            {
                var principal = AuthHelper.ValidateToken(req);
                if (!AuthHelper.HasScope(principal, "hoteach:default"))
                {
                    return new UnauthorizedResult();
                }
                logger.LogInformation($"Authenticated user: {principal.Identity.Name}");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return new UnauthorizedResult();
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty.");
            }
            var data = JsonConvert.DeserializeObject<ActivationRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.UserId))
            {
                return new BadRequestObjectResult("Invalid body. Parameters are required.");
            }

            var payment = await paymentRepository.GetFirstOrDefaultAsync(x => x.UserId == data.UserId);
            if ((payment == null) || string.IsNullOrWhiteSpace(payment.PaymentIntentId))
            {
                return new NotFoundObjectResult("Payment not found.");
            }

            var domain = Environment.GetEnvironmentVariable("Auth0Domain");
            var clientId = Environment.GetEnvironmentVariable("Auth0ClientId");
            var clientSecret = Environment.GetEnvironmentVariable("Auth0ClientSecret");
            var audience = Environment.GetEnvironmentVariable("Auth0Audience");

            var authClient = new AuthenticationApiClient(domain);
            AccessTokenResponse tokenResponse = new AccessTokenResponse();
            try
            {
                tokenResponse = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Audience = audience
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var mgmtClient = new ManagementApiClient(token: tokenResponse.AccessToken, domain: domain);

            var allRoles = await mgmtClient.Roles.GetAllAsync(new GetRolesRequest());
            var rolesToAssign = allRoles.Where(r => r.Name == "Activated");

            if (!rolesToAssign.Any())
                return new NotFoundObjectResult("Specified roles not found.");

            await mgmtClient.Users.AssignRolesAsync(data.UserId, new AssignRolesRequest
            {
                Roles = rolesToAssign.Select(r => r.Id).ToArray()
            });

            logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(data.UserId);
        }
    }
}
