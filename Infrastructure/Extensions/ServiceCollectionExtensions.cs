using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoTeach.Infrastructure.Configuration;
using HoTeach.Infrastructure.Interfaces;

namespace HoTeach.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDatabase(this IServiceCollection services,
            Action<MongoConfiguration> configuration)
        {
            var mongoConfig = new MongoConfiguration();
            configuration(mongoConfig);

            IConvention ignoreIfDefaultOrNullConvention = mongoConfig.IgnoreIfDefaultConvention
                ? new IgnoreIfDefaultConvention(true)
                : new IgnoreIfNullConvention(mongoConfig.IgnoreIfNullConvention);

            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(mongoConfig.EnumConvention),
                ignoreIfDefaultOrNullConvention,
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register("conventionPack", conventionPack, t => true);

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoConfig.ConnectionString));

            var client = new MongoClient(settings);
            var database = client.GetDatabase(mongoConfig.Database);

            services.AddSingleton(database);
            services.AddSingleton(typeof(IMongoClient), p => client);
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

            services.Configure(configuration);

            BsonClassMap.RegisterClassMap<MongoEntity>(p =>
            {
                p.AutoMap();
                p.SetIgnoreExtraElements(true);
            });

            return services;
        }
    }
}
