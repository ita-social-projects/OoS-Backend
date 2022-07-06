using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure;

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
    Task<List<InstitutionHierarchyDto>> GetAll();

    /// <summary>
    /// Get all children of InstitutionHierarchy objects using Redis.
    /// </summary>
    /// <param name="parentId">Key in the table for parent field.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetChildren(Guid? parentId);

    /// <summary>
    /// Get all children of InstitutionHierarchy objects from DB.
    /// </summary>
    /// <param name="parentId">Key in the table for parent field.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetChildrenFromDatabase(Guid? parentId);

    /// <summary>
    /// Get all parents of InstitutionHierarchy objects using Redis.
    /// </summary>
    /// <param name="childId">Key in the table for child field.</param>
    /// <param name="includeCurrentLevel">Set 'true' if there is a need to include current child's level to result.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetParents(Guid childId, bool includeCurrentLevel);

    /// <summary>
    /// Get all parents of InstitutionHierarchy objects from DB.
    /// </summary>
    /// <param name="childId">Key in the table for child field.</param>
    /// <param name="includeCurrentLevel">Set 'true' if there is a need to include current child's level to result.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetParentsFromDatabase(Guid childId, bool includeCurrentLevel);

    /// <summary>
    /// To recieve the InstitutionHierarchy object with define id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="InstitutionHierarchyDto"/> that was found.</returns>
    Task<InstitutionHierarchyDto> GetById(Guid id);

    /// <summary>
    /// Get all InstitutionHierarchy objects by institution id and level using Redis.
    /// </summary>
    /// <param name="institutionId">Key in the table for Institution.</param>
    /// <param name="hierarchyLevel">Hierarchy level for InstitutionHierarchy objects.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetAllByInstitutionAndLevel(Guid institutionId, int hierarchyLevel);

    /// <summary>
    /// Get all InstitutionHierarchy objects by institution id and level from DB.
    /// </summary>
    /// <param name="institutionId">Key in the table for Institution.</param>
    /// <param name="hierarchyLevel">Hierarchy level for InstitutionHierarchy objects.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="InstitutionHierarchyDto"/> that were found.</returns>
    Task<List<InstitutionHierarchyDto>> GetAllByInstitutionAndLevelFromDatabase(Guid institutionId, int hierarchyLevel);

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