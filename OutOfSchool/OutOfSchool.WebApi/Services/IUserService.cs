﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all Users.</returns>
        Task<IEnumerable<UserDto>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>User.</returns>
        Task<UserDto> GetById(string id);
    }
}
