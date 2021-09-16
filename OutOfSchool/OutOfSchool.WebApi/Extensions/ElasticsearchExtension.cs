using System;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using IdentityModel;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Config;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchExtension
    {
        private const int CheckConnectivityDelayMs = 10000;
        private const int Minute = 60;

        /// <summary>
        /// Use this method to add ElasticsearchClient to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>>.
        /// Client lifetime is Singleton.
        /// Creates indices.
        /// </summary>
        /// <param name="services"><see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>> object.</param>
        /// <param name="config"><see cref="OutOfSchool.WebApi.Config.ElasticConfig"/>> object.</param>
        /// <exception cref="ArgumentNullException">If any parameter was not set to instance.</exception>
        public static void AddElasticsearch(this IServiceCollection services, ElasticConfig config)
        {
            if (services is null || config is null)
            {
                throw new ArgumentNullException($"One of the parameters ({services} or {config}) was not set to instance.");
            }

            var uris = config.Urls.Select(url => new Uri(url));
            var pool = new StaticConnectionPool(uris);

            var settings = new ConnectionSettings(pool)
                    .DefaultIndex(config.DefaultIndex)
                    .BasicAuthentication(config.User, config.Password)
                    .EnableDebugMode();

            AddDefaultMappings(settings, config.DefaultIndex);

            var client = new ElasticClient(settings);

            services.AddSingleton(client);

            if (!config.EnsureIndex)
            {
                return;
            }

            EnsureIndexCreated<WorkshopES>(client, config.DefaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings, string indexName)
        {
            settings
                .DefaultMappingFor<WorkshopES>(m => m.IndexName(indexName));
        }

        /// <summary>
        /// The method checks if the index with the specified name exists in the Elasticsearch and creates it if not.
        /// The created index will be strongly typed with the specified type.
        /// </summary>
        /// <typeparam name="T">The type for strongly typed queries.</typeparam>
        /// <param name="client">Elasticsearch client.</param>
        /// <param name="indexName">Name of the index.</param>
        private static void EnsureIndexCreated<T>(IElasticClient client, string indexName)
            where T : class
        {
            // TODO: Remove this later, as Opensearch allows to ensure index exists
            var startTime = DateTime.UtcNow.ToEpochTime();

            while (!client.Ping().IsValid && (DateTime.UtcNow.ToEpochTime() - startTime < Minute))
            {
                // TODO: Use normal logger
                Console.WriteLine("Waiting for Elastic connection");
                Task.Delay(CheckConnectivityDelayMs).Wait();
            }

            var resp = client.Indices.Exists(indexName);

            if (resp.ApiCall.HttpStatusCode == 404)
            {
                client.Indices.Create(indexName, index => index.Map<T>(x => x.AutoMap<T>()));
            }
        }
    }
}
