using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchExtensions
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

            var defaultIndex = nameof(Workshop).ToLower(System.Globalization.CultureInfo.CurrentCulture);

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .BasicAuthentication(user, password);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton(client);

            EnsureIndexCreated<Workshop>(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<Workshop>(m =>
                m.IndexName(nameof(Workshop).ToLower(System.Globalization.CultureInfo.CurrentCulture)));
        }

        private static void EnsureIndexCreated<T>(IElasticClient client, string indexName)
            where T : class
        {
            var resp = client.Indices.Exists(indexName);

            if (resp.ApiCall.HttpStatusCode == 404 && !resp.Exists)
            {
                var createIndexResponse = client.Indices.Create(
                    indexName,
                    index => index.Map<T>(x => x.AutoMap<T>()));
            }
        }
    }
}
