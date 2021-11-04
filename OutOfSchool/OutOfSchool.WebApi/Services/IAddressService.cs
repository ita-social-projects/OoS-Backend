using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Address entity.
    /// </summary>
    public interface IAddressService : ICRUDService<AddressDto, long>
    {
        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        Task<IEnumerable<AddressDto>> GetAll();
    }
}
