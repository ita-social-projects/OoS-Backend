using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure
{
    /// <summary>
    /// Defines interface for CRUD functionality for Institution entity.
    /// </summary>
    public interface IInstitutionService
    {
        /// <summary>
        /// Get all Institution objects using Redis.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionDto"/> that were found.</returns>
        Task<List<InstitutionDto>> GetAll();

        /// <summary>
        /// Get all Institution objects from DB.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionDto"/> that were found.</returns>
        Task<List<InstitutionDto>> GetAllFromDatabase();
    }
}
