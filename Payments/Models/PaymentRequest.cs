namespace HoTeach.Payments.Models
{
    public class PaymentRequest
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public string? Email { get; set; }
    }
} 