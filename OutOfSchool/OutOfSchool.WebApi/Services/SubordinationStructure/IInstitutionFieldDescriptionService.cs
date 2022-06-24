using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure
{
    /// <summary>
    /// Defines interface for CRUD functionality for InstitutionFieldDescription entity.
    /// </summary>
    public interface IInstitutionFieldDescriptionService
    {
        /// <summary>
        /// Get entities by Institution id using Redis.
        /// </summary>
        /// <param name="id">Key of the Institution entity in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionFieldDescriptionDto"/> that were found.</returns>
        Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionId(Guid id);

        /// <summary>
        /// Get entities by Institution id from DB.
        /// </summary>
        /// <param name="id">Key of the Institution entity in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionFieldDescriptionDto"/> that were found.</returns>
        Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionIdFromDatabase(Guid id);
    }
}
