using HoTeach.Infrastructure;

namespace HoTeach.Entities
{
    public class UserPreferences : MongoEntity
    {

        public string UserId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string AgeGroup { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Language { get; set; } = default!;
        public string Education { get; set; } = default!;
        public string Goals { get; set; } = default!;
        public string LearningStyle { get; set; } = default!;
        public string Pace { get; set; } = default!;
        public string JobRole { get; set; } = default!;
        public string SkillLevel { get; set; } = default!;
        public string TimeAvailability { get; set; } = default!;
        public string Schedule { get; set; } = default!;
        public List<string> Motivators { get; set; } = [];
        public List<string> ProgrammingLanguages { get; set; } = [];
        public List<string> Technologies { get; set; } = [];
    }
}
