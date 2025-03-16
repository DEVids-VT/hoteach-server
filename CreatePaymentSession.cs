using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HoteachServer.Payments.Services;
using HoteachServer.Payments.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HoTeach
{
    public class CreatePaymentSession
    {
        private readonly ILogger<CreatePaymentSession> _logger;

        public CreatePaymentSession(ILogger<CreatePaymentSession> logger)
        {
            _logger = logger;
        }

        [Function("CreatePaymentSession")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PaymentSession")] HttpRequest req)
        {
            try
            {
                var principal = AuthHelper.ValidateToken(req);
                if (!AuthHelper.HasScope(principal, "hoteach:default"))
                {
                    return new UnauthorizedObjectResult(HttpStatusCode.Unauthorized);
                }
                _logger.LogInformation($"Authenticated user: {principal.Identity?.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return new UnauthorizedObjectResult(HttpStatusCode.Unauthorized);
            }

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<PaymentRequest>(requestBody);

                if (request == null)
                {
                    return new BadRequestObjectResult(HttpStatusCode.BadRequest);
                }

                var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
                var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");

                

                var stripeService = new StripeService(stripeSecretKey, webhookSecret);
                var paymentResponse = await stripeService.CreatePaymentSessionAsync(request);

        
                return new OkObjectResult(paymentResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment session: {ex.Message}");
                return new BadRequestObjectResult(new { error = ex.Message }); ;
            }
        }
    }
}
