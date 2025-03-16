using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using HoTeach.Entities;
using HoTeach.Infrastructure;  // Assuming you have your MongoDB setup here
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using MongoDB.Driver;
using System.Linq;

namespace HoTeach
{
    public class AddUserPreferences
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<UserPreferences> _userPreferencesCollection;

        public AddUserPreferences(ILogger<AddUserPreferences> logger)
        {
            _logger = logger;
            var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("Mongo__Url"));
            var database = mongoClient.GetDatabase("hoteach-v1"); 
            _userPreferencesCollection = database.GetCollection<UserPreferences>("userpreferences");
        }

        [Function("AddUserPreferences")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user-preferences")] HttpRequest req)
        {
            try
            {
               
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var preferences = JsonSerializer.Deserialize<UserPreferences>(requestBody);

                
                if (preferences == null || string.IsNullOrEmpty(preferences.UserId))
                {
                    return new BadRequestObjectResult("Invalid user preferences data");
                }

                
                await _userPreferencesCollection.InsertOneAsync(preferences);

                
                _logger.LogInformation($"Successfully added user preferences for UserId: {preferences.UserId}");

               
                return new OkObjectResult(new { message = "User preferences added successfully", data = preferences });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding user preferences: {ex.Message}");
                return new StatusCodeResult(500); 
            }
        }
    }
}
