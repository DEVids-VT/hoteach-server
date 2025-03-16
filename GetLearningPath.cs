using HoTeach.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using HoTeach.Entities;
using HoTeach.Requests;
using Newtonsoft.Json;

namespace HoTeach
{
    public class GetLearningPath
    {
        private readonly ILogger<GetLearningPath> _logger;
        private readonly IRepository<HoTeach.Entities.Path> pathRepository;

        public GetLearningPath(ILogger<GetLearningPath> logger, IRepository<HoTeach.Entities.Path> pathRepository)
        {
            _logger = logger;
            this.pathRepository = pathRepository;
        }

        [Function("GetLearningPath")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {

            try
            {
                var principal = AuthHelper.ValidateToken(req);
                if (!AuthHelper.HasScope(principal, "hoteach:default"))
                {
                    return new UnauthorizedResult();
                }
                _logger.LogInformation($"Authenticated user: {principal.Identity.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty.");
            }
            var data = JsonConvert.DeserializeObject<GetPathRequest>(requestBody);

            if (data == null || string.IsNullOrEmpty(data.UserId))
                return new BadRequestObjectResult("UserId is required.");

            var path = await pathRepository.GetFirstOrDefaultAsync(x => x.UserId == data.UserId) ?? null;

            return new OkObjectResult(path );
        }
    }
}
