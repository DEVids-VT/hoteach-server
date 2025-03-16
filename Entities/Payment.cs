using HoTeach.Infrastructure;

namespace HoTeach.Entities;

public class Payment : MongoEntity
{
    public string UserId { get; set; } = default!;
    public string PaymentIntentId { get; set; } = default!;
}