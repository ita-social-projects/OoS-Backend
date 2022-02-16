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
        /// Creates a new teacher without the transaction.
        /// </summary>
        /// <param name="dto">Teacher to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherCreationResultDto> CreateWithoutTransaction(TeacherCreationDto dto);

        /// <summary>
        /// Creates a new teacher.
        /// </summary>
        /// <param name="dto">Teacher to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherCreationResultDto> Create(TeacherCreationDto dto);

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
        /// Updates a teacher without the transaction.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherUpdateResultDto> UpdateWithoutTransaction(TeacherUpdateDto dto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<TeacherUpdateResultDto> Update(TeacherUpdateDto dto);

        /// <summary>
        /// Deletes a teacher without the transaction.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task DeleteWithoutTransaction(Guid id);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);

        /// <summary>
        /// Gets Id of workshop, where specified teacher was created.
        /// </summary>
        /// <param name="teacherId">Teacher's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> GetTeachersWorkshopId(Guid teacherId);
    }
}