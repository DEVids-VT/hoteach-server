namespace HoTeach.Payments.Models
{
    public class PaymentResponse
    {
        public required string SessionUrl { get; set; }
        public required string SessionId { get; set; }
        public required string CustomerId { get; set; }
    }
} 