using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using HoteachServer.Payments.Services;
using System.Net;
using System.Text.Json;

namespace HoTeach
{
    public class StripeWebhook
    {
        private readonly ILogger<StripeWebhook> _logger;

        public StripeWebhook(ILogger<StripeWebhook> logger)
        {
            _logger = logger;
        }

        [Function("StripeWebhook")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stripe-webhook")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            try
            {
                if (!req.Headers.TryGetValues("Stripe-Signature", out var signatureValues))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteAsJsonAsync(new { error = "No signature found" });
                    return response;
                }

                var signature = signatureValues.FirstOrDefault();
                var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
                var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");

                if (string.IsNullOrEmpty(stripeSecretKey) || string.IsNullOrEmpty(webhookSecret))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    await response.WriteAsJsonAsync(new { error = "Stripe configuration is missing" });
                    return response;
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var stripeService = new StripeService(stripeSecretKey, webhookSecret);
                var stripeEvent = stripeService.VerifyWebhook(requestBody, signature);

                switch (stripeEvent.Type)
                {
                    case Events.CheckoutSessionCompleted:
                        if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
                        {
                            await HandleSuccessfulPayment(session);
                        }
                        else
                        {
                            _logger.LogWarning("Invalid session object in checkout.session.completed event");
                        }
                        break;
                    default:
                        _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                        break;
                }

                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error: {ex.Message}");
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteAsJsonAsync(new { error = ex.Message });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        private async Task HandleSuccessfulPayment(Stripe.Checkout.Session session)
        {
            _logger.LogInformation($"Payment successful for customer {session.CustomerId}");
            await Task.CompletedTask;
        }
    }
}
