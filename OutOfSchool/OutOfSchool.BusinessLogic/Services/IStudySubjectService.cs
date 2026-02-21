using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services;
public interface IStudySubjectService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="providerId">Provider Id.</param>
    /// <param name="filter">Filter for list of study subjects</param>
    /// <returns>List of all Subjects.</returns>
    Task<SearchResult<StudySubjectDto>> GetByFilter(Guid providerId, StudySubjectFilter filter);

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

    /// <summary>
    /// Attaches or detaches workshops from a specified study subject based on their attachment status.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider performing the operation.</param>
    /// <param name="workshopsWithStatus">A collection of workshops with their attachment status. 
    /// If <c>IsAttached</c> is <c>true</c>, the workshop will be detached; otherwise, it will be attached.</param>
    /// <returns>
    /// A <see cref="Result{StudySubjectDto}"/> indicating the success or failure of the operation,
    /// along with the updated study subject data if successful.
    /// </returns>
    Task<Result<StudySubjectDto>> UpdateWorkshopsForStudySubject(
            Guid studySubjectId,
            Guid providerId,
            IEnumerable<WorkshopAttachmentStatusDto> workshopsWithStatus);

    /// <summary>
    /// Detaches all workshops from a specified study subject.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider performing the operation.</param>
    /// <returns>
    /// A <see cref="Result{StudySubjectDto}"/> indicating the success or failure of the operation.
    /// </returns>
    Task<Result<StudySubjectDto>> DetachAllWorkshops(Guid studySubjectId, Guid providerId);
}
