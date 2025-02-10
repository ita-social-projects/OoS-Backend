using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;

namespace OutOfSchool.BusinessLogic.Services;
public interface IStudySubjectService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="providerId">Provider Id.</param>
    /// <param name="filter">Filter for list of study subjects</param>
    /// <returns>List of all Subjects.</returns>
    Task<SearchResult<StudySubjectDto>> GetByFilter(Guid providerId, SearchStringFilter filter);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="providerId">Provider Id.</param>
    /// <returns>Subject.</returns>
    Task<StudySubjectDto> GetById(Guid id, Guid providerId);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Subject entity entity to add.</param>
    /// <param name="providerId">Provider Id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<StudySubjectDto> Create(StudySubjectCreateUpdateDto dto, Guid providerId);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">Subject entity to add.</param>
    /// <param name="providerId">Provider Id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Result<StudySubjectDto>> Update(StudySubjectCreateUpdateDto dto, Guid providerId);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Subject Id.</param>
    /// <param name="providerId">Provider Id.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Result<StudySubjectDto>> Delete(Guid id, Guid providerId);
}
