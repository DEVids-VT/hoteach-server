using HoTeach.Entities;
using HoTeach.Infrastructure.Interfaces;
using HoTeach.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI.Chat;
using Path = HoTeach.Entities.Path;

namespace HoTeach
{
    public class GenerateLearningPlan(ChatClient client, IRepository<UserPreferences> prefRepo, IRepository<Path> pathRepo, ILogger<GenerateLearningPlan> logger)
    {
       

        [Function("GenerateLearningPlan")]
        public async  Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                var principal = AuthHelper.ValidateToken(req);
                if (!AuthHelper.HasScope(principal, "hoteach:default"))
                {
                    return new UnauthorizedResult();
                }
                logger.LogInformation($"Authenticated user: {principal.Identity.Name}");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty.");
            }
            var data = JsonConvert.DeserializeObject<GeneratePathRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.UserId))
                return new BadRequestObjectResult("UserId is required.");

            var preferences = await prefRepo.GetFirstOrDefaultAsync(p => p.UserId == data.UserId);

            if (preferences is null)
                return new NotFoundObjectResult("Preferences not found.");

            var systemMessage = GeneratePrompt.SystemMessage();
            var userMessage = GeneratePrompt.UserMessage(preferences);

            var messages = new List<ChatMessage>
             {
                 new SystemChatMessage(systemMessage),
                 new UserChatMessage(userMessage)
             };

            var response = await client.CompleteChatAsync(messages);
            var responseText = response.Value.Content[0].Text;

            await pathRepo.InsertAsync(new Path
            {
                Request = userMessage,
                PathContent = responseText,
                UserId = data.UserId
            });

            return new OkObjectResult(responseText);
        }
    }
}
