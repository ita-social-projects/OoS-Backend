using System;
using Nest;

namespace OutOfSchool.WebApi.Config.Elasticsearch;

/// <summary>
/// Contains methods to configure models into Elasticsearch index.
/// </summary>
public interface IElasticsearchEntityTypeConfiguration
{
    /// <summary>
    /// Configures the particular model into Elasticsearch index.
    /// </summary>
    /// <param name="indexDescriptor">descriptor for configuring.</param>
    /// <returns>An instance of <see cref="ICreateIndexRequest"/>.</returns>
    ICreateIndexRequest Configure(CreateIndexDescriptor indexDescriptor);
}