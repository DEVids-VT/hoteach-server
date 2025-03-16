using Stripe;
using Stripe.Checkout;
using HoteachServer.Payments.Models;

namespace HoteachServer.Payments.Services
{
    public class StripeService
    {
        private readonly string _stripeSecretKey;
        private readonly string _webhookSecret;

        public StripeService(string stripeSecretKey, string webhookSecret)
        {
            _stripeSecretKey = stripeSecretKey;
            _webhookSecret = webhookSecret;
            StripeConfiguration.ApiKey = stripeSecretKey;
        }

        public async Task<PaymentResponse> CreatePaymentSessionAsync(PaymentRequest request)
        {
            var customer = await CreateCustomerAsync(request);
            var session = await CreateCheckoutSessionAsync(customer.Id);

            return new PaymentResponse
            {
                SessionUrl = session.Url,
                SessionId = session.Id,
                CustomerId = customer.Id
            };
        }

        private async Task<Customer> CreateCustomerAsync(PaymentRequest request)
        {
            var options = new CustomerCreateOptions
            {
                Name = request.Username,
                Email = request.Email
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);
        }

        private async Task<Session> CreateCheckoutSessionAsync(string customerId)
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = "price_1R3CIoHB7WdMGy8TtQIBRN3Z", // Replace with your actual price ID
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://your-domain.com/success",
                CancelUrl = "https://your-domain.com/cancel"
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public Event VerifyWebhook(string payload, string signature)
        {
            return EventUtility.ConstructEvent(
                payload,
                signature,
                _webhookSecret,
                throwOnApiVersionMismatch: false
            );
        }
    }
} 