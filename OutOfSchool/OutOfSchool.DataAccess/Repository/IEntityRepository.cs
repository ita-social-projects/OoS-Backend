using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    /// <summary>
    /// Interface of repository for accessing the database.
    /// </summary>
    /// <typeparam name="T">Entity.</typeparam>
    public interface IEntityRepository<T>
        where T : class, new()
    {
        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> Create(T entity);


        /// <summary>
        /// Update information about element.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> Update(T entity);


        /// <summary>
        /// Delete element.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(T entity);
	
        /// <summary>
        /// Get all elements.
        /// </summary>
        /// <returns>List of all elements.</returns>
        Task<IEnumerable<T>> GetAll();


        /// <summary>
        /// Get all elements with details.
        /// </summary>
        /// <param name="includeProperties">Name of properties which should be included.</param>
        /// <returns>List of all elements with included propertires.</returns>
        Task<IEnumerable<T>> GetAllWithDetails(string includeProperties = "");


        /// <summary>
        /// Get element by Id.
        /// </summary>
        /// <param name="id">Key in database.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<T> GetById(long id);

        /// <summary>
        /// Get element by id with details.
        /// </summary>
        /// <param name="predicate">Filter with key.</param>
        /// <param name="includeProperties">Name of properties which should be included.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<T>> GetAllWIthDetails(Expression<Func<T, bool>> predicate, string includeProperties = "");
    }
}
