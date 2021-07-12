using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// An interface for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TSearch">The filter type.</typeparam>
    public interface IElasticsearchProvider<TEntity, TSearch>
        where TEntity : class, new()
        where TSearch : class, new()
    {
        /// <summary>
        /// Use this method to add entity to the index.
        /// </summary>
        /// <param name="entity">The entity that will be stored as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task IndexEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to update entity in the index.
        /// </summary>
        /// <param name="entity">The entity that will be updated as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task UpdateEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to delete entity from the index.
        /// </summary>
        /// <param name="entity">The entity that will be deleted from the index.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task DeleteEntityAsync(TEntity entity);

        /// <summary>
        /// Use this method to delete all entities from the index.
        /// And then to add all entities from the source of truth.
        /// </summary>
        /// <param name="source">The source from which entities will be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task ReIndexAll(IEnumerable<TEntity> source);

        /// <summary>
        /// Use this method to search entities that match the filter's parameters.
        /// </summary>
        /// <param name="filter">The filter parameters.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the entities that were found.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task<IEnumerable<TEntity>> Search(TSearch filter);
    }
}
