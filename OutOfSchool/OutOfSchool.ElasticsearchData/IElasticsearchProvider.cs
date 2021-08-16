using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// An interface for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that will be used for strongly typed queries.</typeparam>
    /// <typeparam name="TSearch">The type of filter for searching purposes.</typeparam>
    public interface IElasticsearchProvider<TEntity, TSearch>
        where TEntity : class, new()
        where TSearch : class, new()
    {
        /// <summary>
        /// Use this method to add entity to the index.
        /// </summary>
        /// <param name="entity">The entity that will be stored as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the status of operation that was proceeded in Elasticsearch.
        /// If entity Id does not exist in the base than the status will be <see cref="Result.Created"/>.
        /// If entity with specified Id already exists in the base than the status will be <see cref="Result.Updated"/>.
        /// In case of any error occurs the status will be <see cref="Result.Error"/>.</returns>
        Task<Result> IndexEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to update entity in the index.
        /// </summary>
        /// <param name="entity">The entity that will be updated as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the status of operation that was proceeded in Elasticsearch.
        /// If entity Id does not exist in the base than the status will be <see cref="Result.Created"/>.
        /// If entity with specified Id already exists in the base than the status will be <see cref="Result.Updated"/>.
        /// If entity with specified Id is already updated in the base than the status will be <see cref="Result.Noop"/>.
        /// In case of any error occurs the status will be <see cref="Result.Error"/>.</returns>
        Task<Result> UpdateEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to delete entity from the index.
        /// </summary>
        /// <param name="entity">The entity that will be deleted from the index.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the status of operation that was proceeded in Elasticsearch.
        /// If entity Id does not exist in the base than the status will be <see cref="Result.NotFound"/>.
        /// If entity with specified Id was successfuly deleted from the base than the status will be <see cref="Result.Deleted"/>.
        /// In case of any error occurs the status will be <see cref="Result.Error"/>.</returns>
        Task<Result> DeleteEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to delete all entities from the index.
        /// And then to add all entities from the source of truth.
        /// The internal implementation is based on BulkAll-method.
        /// </summary>
        /// <param name="source">The source from which entities will be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the status of operation that was proceeded in Elasticsearch.
        /// If successfull status will be <see cref="Result.Updated"/>.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task<Result> ReIndexAll(IEnumerable<TEntity> source);

        /// <summary>
        /// Use this method to add/update bulk of entities from the source.
        /// The internal implementation is based on BulkAll-method.
        /// </summary>
        /// <param name="source">The source from which entities will be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the status of operation that was proceeded in Elasticsearch.
        /// If successfull status will be <see cref="Result.Updated"/>.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task<Result> IndexAll(IEnumerable<TEntity> source);

        /// <summary>
        /// Use this method to search entities that match the filter's parameters.
        /// If the filter is null, than filter with default values will be used.
        /// If the method is not overrided in the typed inheritor than all entites shoud be returned.
        /// </summary>
        /// <param name="filter">The filter parameters.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the entities that were found.</returns>
        Task<SearchResultES<TEntity>> Search(TSearch filter = null);

        /// <summary>
        /// Use this method to check if Elasticsearch is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains true if server is available.</returns>
        Task<bool> PingServerAsync();
    }
}
