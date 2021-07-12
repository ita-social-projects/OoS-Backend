using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// A provider for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TSearch">The filter type.</typeparam>
    public class ElasticsearchProvider<TEntity, TSearch> : IElasticsearchProvider<TEntity, TSearch>
        where TEntity : class, new()
        where TSearch : class, new()
    {
        public ElasticsearchProvider(ElasticClient elasticClient)
        {
            this.ElasticClient = elasticClient;
        }

        protected ElasticClient ElasticClient { get; private set; }

        /// <inheritdoc/>
        public async Task IndexEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.IndexDocumentAsync(entity);

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task UpdateEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.UpdateAsync<TEntity>(entity, u => u.Doc(entity));

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task DeleteEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.DeleteAsync<TEntity>(entity);

            CheckResponse(resp);
        }

        /// <inheritdoc/>
        public async Task ReIndexAll(IEnumerable<TEntity> source)
        {
            await ElasticClient.DeleteByQueryAsync<TEntity>(q => q.MatchAll());

            foreach (var entity in source)
            {
                var resp = await ElasticClient.IndexDocumentAsync(entity);
                CheckResponse(resp);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TEntity>> Search(TSearch filter)
        {
            var resp = await ElasticClient.SearchAsync<TEntity>(
                    s => s.Query(
                        q => q.MatchAll()));

            CheckResponse(resp);

            return resp.Documents;
        }

        private void CheckResponse(IResponse resp)
        {
            if (!resp.IsValid)
            {
                throw resp.OriginalException;
            }
        }
    }
}
