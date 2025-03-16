using HoTeach.Infrastructure;

namespace HoTeach.Entities
{
    public class Path : MongoEntity
    {
        public string UserId { get; set; } = default!;
        public string Request { get; set; } = default!;
        public string PathContent { get; set; } = default!;
    }
}
