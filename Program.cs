using HoTeach.Infrastructure.Configuration;
using HoTeach.Infrastructure.Extensions;
using HoTeach.Payments.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using OpenAI.Chat;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://dev-m31s5020w8rygw8y.us.auth0.com";
        options.Audience = "https://hoteachaudience.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://dev-m31s5020w8rygw8y.us.auth0.com",
            ValidateAudience = true,
            ValidAudience = "https://hoteachaudience.com",
            ValidateLifetime = true,
        };
    });

builder.Services.AddSingleton(new ChatClient(model: "gpt-4o-mini", Environment.GetEnvironmentVariable("OpenAIApiKey")));

var mongoConfig = builder.Configuration.GetSection("Mongo");
string mongoUrl = mongoConfig["Url"];
string databaseName = mongoConfig["Database"];
builder.Services.AddMongoDatabase(p =>
{
    p.WithConnectionString(mongoUrl);
    p.WithDatabaseName(databaseName);
    p.WithSoftDeletes(o =>
    {
        o.Enabled(true);
    });
    p.RepresentEnumValuesAs(BsonType.String);
    p.WithIgnoreIfDefaultConvention(false);
    p.WithIgnoreIfNullConvention(true);
});
//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
