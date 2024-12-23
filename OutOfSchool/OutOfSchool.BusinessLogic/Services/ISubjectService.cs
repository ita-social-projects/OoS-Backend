using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;
public interface ISubjectService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all Subjects.</returns>
    Task<IEnumerable<StudySubjectDto>> GetAll(LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// /// <param name="id">Key in the table.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param> 
    /// <returns>Subject.</returns>
    Task<StudySubjectDto> GetById(long id, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Tag entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<StudySubjectDto> Create(StudySubjectDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// /// <param name="dto">Subject entity to add.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<StudySubjectDto> Update(StudySubjectDto dto, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Subject key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}
