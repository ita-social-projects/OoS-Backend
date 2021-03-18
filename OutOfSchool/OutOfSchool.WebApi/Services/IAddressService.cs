using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Address entity.
    /// </summary>
    public interface IAddressService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Address entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AddressDto> Create(AddressDto dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all Addresses .</returns>
        Task<IEnumerable<AddressDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Address.</returns>
        Task<AddressDto> GetById(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Address entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AddressDto> Update(AddressDto dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Address key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
