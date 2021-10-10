using System;
using System.Threading.Tasks;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// An interface for the Elasticsearch client.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TSearch">The filter type.</typeparam>
    public interface IElasticsearchService<TEntity, TSearch>
        where TEntity : class, new()
        where TSearch : class, new()
    {
        /// <summary>
        /// Use this method to add entity to the index.
        /// </summary>
        /// <param name="entity">The entity that will be stored as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Index(TEntity entity);

        /// <summary>
        /// Use this method to update entity in the index.
        /// </summary>
        /// <param name="entity">The entity that will be updated as a document.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Update(TEntity entity);

        /// <summary>
        /// Use this method to delete entity from the index.
        /// </summary>
        /// <param name="id">The entity's key that will be deleted from the index.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> Delete(Guid id);

        /// <summary>
        /// Use this method to delete all entities from the index.
        /// And then to add all entities from the source of truth.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> ReIndex();

        /// <summary>
        /// Use this method to see if Elasticsearch server is availiable.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<bool> PingServer();

        /// <summary>
        /// Use this method to search entities that match the filter's parameters.
        /// </summary>
        /// <param name="filter">The filter parameters.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains the entities that were found.</returns>
        Task<SearchResultES<TEntity>> Search(TSearch filter);
    }
}
