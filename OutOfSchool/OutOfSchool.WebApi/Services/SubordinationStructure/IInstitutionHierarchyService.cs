using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.SubordinationStructure
{
    /// <summary>
    /// Defines interface for CRUD functionality for InstitutionHierarchy entity.
    /// </summary>
    public interface IInstitutionHierarchyService
    {
        /// <summary>
        /// Add new InstitutionHierarchy to the DB.
        /// </summary>
        /// <param name="dto">InstitutionHierarchyDto element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="InstitutionHierarchyDto"/> that was created.</returns>
        Task<InstitutionHierarchyDto> Create(InstitutionHierarchyDto dto);

        /// <summary>
        /// Get all InstitutionHierarchy objects from DB.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
        Task<IEnumerable<InstitutionHierarchyDto>> GetAll();

        /// <summary>
        /// Get all InstitutionHierarchy objects from DB.
        /// </summary>
        /// <param name="parentId">Key in the table for parent field.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
        Task<IEnumerable<InstitutionHierarchyDto>> GetChildren(Guid? parentId);

        /// <summary>
        /// To recieve the InstitutionHierarchy object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="InstitutionHierarchyDto"/> that was found.</returns>
        Task<InstitutionHierarchyDto> GetById(Guid id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="dto">InstitutionHierarchy with new properties.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="InstitutionHierarchyDto"/> that was updated.</returns>
        Task<InstitutionHierarchyDto> Update(InstitutionHierarchyDto dto);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key of the InstitutionHierarchy in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Result<InstitutionHierarchyDto>> Delete(Guid id);
    }
}
