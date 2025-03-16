using Stripe;
using Stripe.Checkout;
using HoTeach.Payments.Models;

namespace HoTeach.Payments.Services
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

        public async Task<Customer> GetCustomer(string customerId)
        {
            var service = new CustomerService();

            return await service.GetAsync(customerId);
        }

        private async Task<Customer> CreateCustomerAsync(PaymentRequest request)
        {
            var options = new CustomerCreateOptions
            {
                Name = request.Username,
                Email = request.Email,
                Metadata = new Dictionary<string, string>()
                {
                    {"UserId", request.UserId}
                }
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
                SuccessUrl = "https://app.hoteach.com/activation",
                CancelUrl = "https://app.hoteach.com/"
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