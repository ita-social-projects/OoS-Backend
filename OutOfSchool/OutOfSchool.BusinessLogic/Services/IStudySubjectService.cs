using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;

namespace OutOfSchool.BusinessLogic.Services;
public interface IStudySubjectService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <returns>List of all Subjects.</returns>
    Task<SearchResult<StudySubjectDto>> GetByFilter(SearchStringFilter filter);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// /// <param name="id">Key in the table.</param>
    /// <returns>Subject.</returns>
    Task<StudySubjectDto> GetById(Guid id);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Tag entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<StudySubjectDto> Create(StudySubjectCreateUpdateDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// /// <param name="dto">Subject entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Result<StudySubjectDto>> Update(StudySubjectCreateUpdateDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Subject key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Result<StudySubjectDto>> Delete(Guid id);
}
