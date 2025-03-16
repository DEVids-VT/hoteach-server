namespace HoteachServer.Payments.Models
{
    public class PaymentRequest
    {
        public required string Username { get; set; }
        public string? Email { get; set; }
    }
} 