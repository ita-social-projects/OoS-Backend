using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// A provider for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="T">Name of the entity and its index.</typeparam>
    public class ElasticsearchProvider<T> : IElasticsearchProvider<T>
        where T : class, new()
    {
        private readonly ElasticClient elasticClient;

        public ElasticsearchProvider(ElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        /// <inheritdoc/>
        public async Task IndexEntityAsync(T entity)
        {
            var resp = await elasticClient.IndexDocumentAsync(entity);

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task UpdateEntityAsync(T entity)
        {
            var resp = await elasticClient.UpdateAsync<T>(entity, u => u.Doc(entity));

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task DeleteEntityAsync(T entity)
        {
            var resp = await elasticClient.DeleteAsync<T>(entity);

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task ReIndexAll(IEnumerable<T> source)
        {
            await elasticClient.DeleteByQueryAsync<T>(q => q.MatchAll());

            foreach (var entity in source)
            {
                try
                {
                    var resp = await elasticClient.IndexDocumentAsync(entity);
                    CheckResponse(resp);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> Search(string query)
        {
            ISearchResponse<T> resp;

            if (string.IsNullOrWhiteSpace(query))
            {
                resp = await elasticClient.SearchAsync<T>(
                    s => s.Query(
                        q => q.MatchAll()));
            }

            resp = await elasticClient.SearchAsync<T>(
                s => s.Query(
                    q => q.Match(
                        m => m.Query(query))));

            CheckResponse(resp);

            return resp.Documents;
        }

        private void CheckResponse(IResponse resp)
        {
            if (!resp.IsValid)
            {
                var message = $"Elasticsearch error: {{ \nApi call: {resp.ApiCall}\nOriginal exception: {resp.OriginalException}";
                throw new Exception(message);
            }
        }
    }
}
