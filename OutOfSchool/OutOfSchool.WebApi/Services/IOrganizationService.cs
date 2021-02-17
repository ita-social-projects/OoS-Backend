using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of OrganizationService.
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Add new Organization to the database.
        /// </summary>
        /// <param name="organization">Organization entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Create(OrganizationDTO organization);

        /// <summary>
        /// Get all organizations from the database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        Task<IEnumerable<OrganizationDTO>> GetAll();

        /// <summary>
        /// Get organization by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Organization.</returns>
        Task<OrganizationDTO> GetById(long id);

        /// <summary>
        /// Update information about a specific Organization entity.
        /// </summary>
        /// <param name="organizationDTO">Organization entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Update(OrganizationDTO organizationDTO);

        /// <summary>
        ///  Delete organization from the database by it's key.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
