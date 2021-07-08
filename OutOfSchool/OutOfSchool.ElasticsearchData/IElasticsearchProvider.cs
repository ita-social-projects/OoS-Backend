using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.ElasticsearchData
{
    /// <summary>
    /// An interface for the Elasticsearch API.
    /// </summary>
    /// <typeparam name="T">Name of index.</typeparam>
    public interface IElasticsearchProvider<T>
        where T : class, new()
    {
        /// <summary>
        /// Use this method to add entity to the index with name of T.
        /// </summary>
        /// <param name="entity">The entity that will be stored as a document to with index with name of T.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task IndexEntityAsync(T entity);

        /// <summary>
        /// Use this method to update entity in the index with name of T.
        /// </summary>
        /// <param name="entity">The entity that will be updated as a document in the index with name of T.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task UpdateEntityAsync(T entity);

        /// <summary>
        /// Use this method to delete entity from the index with name of T.
        /// </summary>
        /// <param name="entity">The entity that will be deleted from the index with name of T.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task DeleteEntityAsync(T entity);

        /// <summary>
        /// Use this method to delete all entities from the index with name of T.
        /// And then to add all entities from the source of truth.
        /// </summary>
        /// <param name="source">The source from which entities will be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task ReIndexAll(IEnumerable<T> source);

        /// <summary>
        /// Use this method to search entities that match query parameters.
        /// </summary>
        /// <param name="query">The filter parameters.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the entities that were found.</returns>
        /// <exception cref="Exception">If response from the Elasticsearch server was Invalid.</exception>
        Task<IEnumerable<T>> Search(string query);
    }
}
