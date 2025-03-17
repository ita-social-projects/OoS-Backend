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
    /// Attaches and detaches workshops of a specified study subject.
    /// </summary>
    /// <param name="studySubjectId">The unique identifier of the study subject.</param>
    /// <param name="workshopIdsToAttach">A collection of workshop IDs to attach to the study subject.</param>
    /// <param name="workshopIdsToDetach">A collection of workshop IDs to detach from the study subject.</param>
    /// <param name="providerId">The unique identifier of the provider performing the operation.</param>
    /// <returns>
    /// A <see cref="Result{StudySubjectDto}"/> indicating the success or failure of the operation.
    /// </returns>
    Task<Result<StudySubjectDto>> UpdateWorkshopsForStudySubject(
            Guid studySubjectId,
            IEnumerable<Guid> workshopIdsToAttach,
            IEnumerable<Guid> workshopIdsToDetach,
            Guid providerId);

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
