using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.Tag;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Tag entity.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all Tag.</returns>
    Task<IEnumerable<TagDto>> GetAll(LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// /// <param name="id">Key in the table.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param> 
    /// <returns>Tag.</returns>
    Task<TagDto> GetById(long id, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Tag entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<TagDto> Create(TagCreate dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// /// <param name="dto">Tag entity to add.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<TagDto> Update(TagDto dto, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Tag key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}
