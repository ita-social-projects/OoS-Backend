using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace OutOfSchool.WebApi.Extensions;

public static class ElasticsearchExtension
{
    /// <summary>
    /// Use this method to add ElasticsearchClient to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>>.
    /// Client lifetime is Singleton.
    /// Creates indices.
    /// </summary>
    /// <param name="services"><see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>> object.</param>
    /// <param name="config"><see cref="OutOfSchool.BusinessLogic.Config.ElasticConfig"/>> object.</param>
    /// <exception cref="ArgumentNullException">If any parameter was not set to instance.</exception>
    public static void AddElasticsearch(this IServiceCollection services, ElasticConfig config)
    {
        if (services is null || config is null)
        {
            throw new ArgumentNullException($"One of the parameters ({services} or {config}) was not set to instance.");
        }

        var uris = config.Urls.Select(url => new Uri(url));
        var pool = new StaticNodePool(uris);

        var settings = new ElasticsearchClientSettings(pool)
            .DefaultIndex(config.WorkshopIndexName)
            .Authentication(new BasicAuthentication(config.User, config.Password));

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

                    if (!details.HasSuccessfulStatusCode)
                    {
                        Log.Error($"Reason: {details.OriginalException}");
                    }
                });
        }

        AddDefaultMappings<WorkshopES>(settings, config.WorkshopIndexName);

        var client = new ElasticsearchClient(settings);

        services.AddSingleton(client);
    }

    private static void AddDefaultMappings<TElasticsearchEntity>(ElasticsearchClientSettings settings, string indexName)
        where TElasticsearchEntity : class, new()
    {
        settings
            .DefaultMappingFor<TElasticsearchEntity>(m => m.IndexName(indexName));
    }
}