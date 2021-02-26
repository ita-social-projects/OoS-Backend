﻿using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Interface of Parent Service
    /// </summary>
    public interface IParentService
    {
        /// <summary>
        /// Add new Parent to the DB.
        /// </summary>
        /// <param name="parent">ParentDTO element</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ParentDTO> Create(ParentDTO parent);

        /// <summary>
        /// Get all Parent objects from DB.
        /// </summary>
        /// <returns>List of Parent objects</returns>
        Task<IEnumerable<ParentDTO>> GetAll();

        /// <summary>
        /// To recieve the Parent object with define id.
        /// </summary>
        /// <param name="id">Key in the table</param>
        /// <returns>Parent object</returns>
        Task<ParentDTO> GetById(long id);

        /// <summary>
        /// To Update our object in DB
        /// </summary>
        /// <param name="parent">Parent with new properties</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ParentDTO> Update(ParentDTO parent);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key in table</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
