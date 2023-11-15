using System.Text;
using Elasticsearch.Net;
using Nest;
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
            .BasicAuthentication(config.User, config.Password)
            .EnableApiVersioningHeader();

        if (config.EnableDebugMode)
        {
            settings
                .EnableDebugMode(details =>
                {
                    if (details.RequestBodyInBytes != null)
                    {
                        Log.Debug(
                            $"{details.HttpMethod} {details.Uri} " +
                            $"{Encoding.UTF8.GetString(details.RequestBodyInBytes)}");
                    }
                    else
                    {
                        Log.Debug($"{details.HttpMethod} {details.Uri}");
                    }

                    // log out the response and the response body, if one exists for the type of response
                    if (details.ResponseBodyInBytes != null)
                    {
                        Log.Debug($"Status: {details.HttpStatusCode}" +
                                  $"{Encoding.UTF8.GetString(details.ResponseBodyInBytes)}");
                    }
                    else
                    {
                        Log.Debug($"Status: {details.HttpStatusCode}");
                    }

                    if (!details.Success)
                    {
                        Log.Error($"Reason: {details.OriginalException}");
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
        var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        while (!client.Ping().IsValid && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime < Minute))
        {
            Log.Information("Waiting for Elastic connection");
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