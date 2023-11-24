using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.SocialGroup;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for SocialGroup entity.
/// </summary>
public interface ISocialGroupService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all SocialGroups.</returns>
    Task<IEnumerable<SocialGroupDto>> GetAll(LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <param name="id">Key in the table.</param>
    /// <returns>SocialGroup.</returns>
    Task<SocialGroupDto> GetById(long id, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<SocialGroupCreate> Create(SocialGroupCreate dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<SocialGroupDto> Update(SocialGroupDto dto, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">SocialGroup key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}