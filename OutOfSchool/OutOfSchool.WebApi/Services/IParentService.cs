using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of Parent Service.
    /// </summary>
    public interface IParentService
    {
        /// <summary>
        /// Add new Parent to the DB.
        /// </summary>
        /// <param name="parent">ParentDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ParentDTO> Create(ParentDTO parent);

        /// <summary>
        /// Get all Parent objects from DB.
        /// </summary>
        /// <returns>List of Parent objects.</returns>
        Task<IEnumerable<ParentDTO>> GetAll();

        /// <summary>
        /// Get Parent objects from DB by filter.
        /// </summary>
        /// <param name="filter">Filter for Parent dto.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="ParentDTO"/> that were found.</returns>
        Task<SearchResult<ParentDTO>> GetByFilter(SearchStringFilter filter);

        /// <summary>
        /// To recieve the Parent object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Parent object.</returns>
        Task<ParentDTO> GetById(Guid id);

        /// <summary>
        /// Get entity by User id.
        /// </summary>
        /// <param name="id">Key of the User entity in the table.</param>
        /// <returns>Parent.</returns>
        Task<ParentDTO> GetByUserId(string id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="user">User with new properties.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ShortUserDto> Update(ShortUserDto user);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);
    }
}
