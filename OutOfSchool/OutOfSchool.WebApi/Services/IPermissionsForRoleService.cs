using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface IPermissionsForRoleService
    {
        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all PermissionsForRole.</returns>
        Task<IEnumerable<PermissionsForRoleDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>PermissionsForRole.</returns>
        Task<PermissionsForRoleDTO> GetById(long id);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">PermissionsForRole entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<PermissionsForRoleDTO> Create(PermissionsForRoleDTO dto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">PermissionsForRole entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<PermissionsForRoleDTO> Update(PermissionsForRoleDTO dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">PermissionsForRole key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);

    }
}
