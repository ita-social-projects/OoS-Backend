using System;
using Nest;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// A provider for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="T">Name of index.</typeparam>
    public class ElasticsearchProvider<T> : IElasticsearchProvider<T>
    {
        private ElasticClient elasticClient;

        public ElasticsearchProvider(ElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }
    }
}
