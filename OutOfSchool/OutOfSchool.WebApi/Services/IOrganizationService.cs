using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Organization entity.
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Organization entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Create(OrganizationDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        Task<IEnumerable<OrganizationDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Organization.</returns>
        Task<OrganizationDTO> GetById(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Organization entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Update(OrganizationDTO dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
