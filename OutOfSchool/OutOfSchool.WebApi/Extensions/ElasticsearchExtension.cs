using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Config.Elasticsearch;

namespace OutOfSchool.WebApi.Extensions;

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
            .DefaultIndex(config.WorkshopIndexName)
            .BasicAuthentication(config.User, config.Password);

        if (config.EnableDebugMode)
        {
            settings
                .EnableDebugMode(details =>
                {
                    if (details.RequestBodyInBytes != null)
                    {
                        Console.WriteLine(
                            $"{details.HttpMethod} {details.Uri} " +
                            $"{Encoding.UTF8.GetString(details.RequestBodyInBytes)}");
                    }
                    else
                    {
                        Console.WriteLine($"{details.HttpMethod} {details.Uri}");
                    }

                    // log out the response and the response body, if one exists for the type of response
                    if (details.ResponseBodyInBytes != null)
                    {
                        Console.WriteLine($"Status: {details.HttpStatusCode}" +
                                          $"{Encoding.UTF8.GetString(details.ResponseBodyInBytes)}");
                    }
                    else
                    {
                        Console.WriteLine($"Status: {details.HttpStatusCode}");
                    }

                    if (!details.Success)
                    {
                        Console.Error.WriteLine($"Reason: {details.OriginalException}");
                    }
                });
        }

        AddDefaultMappings<WorkshopES>(settings, config.WorkshopIndexName);

        var client = new ElasticClient(settings);

        services.AddSingleton(client);

        if (!config.EnsureIndex)
        {
            return;
        }

        EnsureIndexCreated(client, config.WorkshopIndexName, new ElasticsearchWorkshopConfiguration());
    }

    private static void AddDefaultMappings<TElasticsearchEntity>(ConnectionSettings settings, string indexName)
        where TElasticsearchEntity : class, new()
    {
        settings
            .DefaultMappingFor<TElasticsearchEntity>(m => m.IndexName(indexName));
    }

    /// <summary>
    /// The method checks if the index with the specified name exists in the Elasticsearch and creates it if not.
    /// The created index will be strongly typed with the specified type.
    /// </summary>
    /// <param name="client">Elasticsearch client.</param>
    /// <param name="indexName">Name of the index.</param>
    /// <param name="configurator">Elasticsearch models configurator.</param>
    private static void EnsureIndexCreated(IElasticClient client, string indexName, IElasticsearchEntityTypeConfiguration configurator)
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

        if (resp.ApiCall.HttpStatusCode == StatusCodes.Status404NotFound)
        {
            client.Indices.Create(
                indexName,
                configurator.Configure);
        }
    }
}