using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    /// <summary>
    /// Interface of OrganizationService.
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Add new Organization to the database.
        /// </summary>
        /// <param name="organization">OrganizationDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Create(OrganizationDTO organization);

        /// <summary>
        /// Get all organizations from database.
        /// </summary>
        /// <returns>List of all organizations.</returns>
        Task<IEnumerable<OrganizationDTO>> GetAll();

        /// <summary>
        /// Get organization by id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Organization.</returns>
        Task<OrganizationDTO> GetById(long id);

        /// <summary>
        /// Update information about element.
        /// </summary>
        /// <param name="organizationDTO"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<OrganizationDTO> Update(OrganizationDTO organizationDTO);


        /// <summary>
        /// Delete some element in database.
        /// </summary>
        /// <param name="id">Element's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
