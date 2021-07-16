using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchExtension
    {
        /// <summary>
        /// Use this method to add ElasticsearchClient to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>>.
        /// Client lifetime is Singleton.
        /// Creates indices.
        /// </summary>
        /// <param name="services"><see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>> object.</param>
        /// <param name="configuration"><see cref="Microsoft.Extensions.Configuration.IConfiguration"/>> object.</param>
        /// <exception cref="ArgumentNullException">If any paramentr was not set to instance.</exception>
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null || configuration is null)
            {
                throw new ArgumentNullException($"One of the parameters ({services} or {configuration}) was not set to instance.");
            }

            var url = configuration["elasticsearch:url"];
            var user = configuration["elasticsearch:user"];
            var password = configuration["elasticsearch:password"];

            var defaultIndex = "workshop";

            var settings = new ConnectionSettings(new Uri(url))
                    .DefaultIndex(defaultIndex)
                    .BasicAuthentication(user, password)
                    .EnableDebugMode();

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton(client);

            EnsureIndexCreated<WorkshopES>(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<WorkshopES>(m => m.IndexName("workshop"));
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
            var resp = client.Indices.Exists(indexName);

            if (resp.ApiCall.HttpStatusCode == 404)
            {
                client.Indices.Create(indexName, index => index.Map<T>(x => x.AutoMap<T>()));
            }
        }
    }
}
