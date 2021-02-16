using OutOfSchool.WebApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    /// <summary>
    /// Interface of ChildService.
    /// </summary>
    public interface IChildService
    {
        /// <summary>
        /// Add new Child to the database.
        /// </summary>
        /// <param name="child">ChildDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Create(ChildDTO child);

        /// <summary>
        /// Get all children from database.
        /// </summary>
        /// <returns>List of all children.</returns>
        IEnumerable<ChildDTO> GetAll();

        /// <summary>
        /// Get child with id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Child.</returns>
        Task<ChildDTO> GetById(long id);

        /// <summary>
        /// Update info in database.
        /// </summary>
        /// <param name="childDTO">Element with new info.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<ChildDTO> Update(ChildDTO childDTO);


        /// <summary>
        /// Delete some element in database.
        /// </summary>
        /// <param name="id">Element's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
