using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// A provider to the Elasticsearch API.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that will be used for strongly typed queries.</typeparam>
    /// <typeparam name="TSearch">The type of filter for searching purposes.</typeparam>
    public class ElasticsearchProvider<TEntity, TSearch> : IElasticsearchProvider<TEntity, TSearch>
        where TEntity : class, new()
        where TSearch : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticsearchProvider{TEntity, TSearch}"/> class.
        /// </summary>
        /// <param name="elasticClient">The configured instance of Elasticsearch client.</param>
        public ElasticsearchProvider(ElasticClient elasticClient)
        {
            this.ElasticClient = elasticClient;
        }

        protected ElasticClient ElasticClient { get; private set; }

        /// <inheritdoc/>
        public virtual async Task<Result> IndexEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.IndexDocumentAsync(entity);

            return resp.Result;
        }

        /// <inheritdoc/>
        public virtual async Task<Result> UpdateEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.UpdateAsync<TEntity>(entity, u => u.Doc(entity).Upsert(entity));

            return resp.Result;
        }

        /// <inheritdoc/>
        public virtual async Task<Result> DeleteEntityAsync(TEntity entity)
        {
            var resp = await ElasticClient.DeleteAsync<TEntity>(entity);

            return resp.Result;
        }

        /// <inheritdoc/>
        public virtual async Task<Result> ReIndexAll(IEnumerable<TEntity> source)
        {
            await ElasticClient.DeleteByQueryAsync<TEntity>(q => q.MatchAll());

            var bulkAllObservable = ElasticClient.BulkAll<TEntity>(source, b => b
                .MaxDegreeOfParallelism(4)
                .BackOffTime("10s")
                .BackOffRetries(2)
                .RefreshOnCompleted()
                .Size(1000));

            var waitHandle = new ManualResetEvent(false);
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            Result result = Result.Error;

            var observer = new BulkAllObserver(
                onError: exception =>
                {
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                    waitHandle.Set();
                },
                onCompleted: () =>
                {
                    result = Result.Updated;
                    waitHandle.Set();
                });

            bulkAllObservable.Subscribe(observer);

            waitHandle.WaitOne();

            exceptionDispatchInfo?.Throw();

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<Result> IndexAll(IEnumerable<TEntity> source)
        {
            var bulkAllObservable = ElasticClient.BulkAll<TEntity>(source, b => b
                .MaxDegreeOfParallelism(4)
                .BackOffTime("10s")
                .BackOffRetries(2)
                .RefreshOnCompleted()
                .Size(1000));

            var waitHandle = new ManualResetEvent(false);
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            Result result = Result.Error;

            var observer = new BulkAllObserver(
                onError: exception =>
                {
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                    waitHandle.Set();
                },
                onCompleted: () =>
                {
                    result = Result.Updated;
                    waitHandle.Set();
                });

            bulkAllObservable.Subscribe(observer);

            waitHandle.WaitOne();

            exceptionDispatchInfo?.Throw();

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<SearchResultES<TEntity>> Search(TSearch filter = null)
        {
            var resp = await ElasticClient.SearchAsync<TEntity>(
                    s => s.Query(
                        q => q.MatchAll()));

            return new SearchResultES<TEntity>() { TotalAmount = (int)resp.Total, Entities = resp.Documents };
        }

        public async Task<bool> PingServerAsync()
        {
            var resp = await ElasticClient.PingAsync();

            return resp.IsValid;
        }
    }
}
