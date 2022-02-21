using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Teacher entity.
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Creates a new teacher.
        /// </summary>
        /// <param name="dto">Teacher to add.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherCreationResultDto> Create(TeacherCreationDto dto, bool enabledTransaction = true);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all teachers.</returns>
        Task<IEnumerable<TeacherDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>Teacher.</returns>
        Task<TeacherDTO> GetById(Guid id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherUpdateResultDto> Update(TeacherUpdateDto dto, bool enabledTransaction = true);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id, bool enabledTransaction = true);

        /// <summary>
        /// Gets Id of workshop, where specified teacher was created.
        /// </summary>
        /// <param name="teacherId">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> GetTeachersWorkshopId(Guid teacherId);
    }
}