using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for SocialGroup entity.
/// </summary>
public interface IProviderTypeService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <returns>List of all SocialGroups.</returns>
    Task<IEnumerable<ProviderTypeDto>> GetAll();

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>SocialGroup.</returns>
    Task<ProviderTypeDto> GetById(long id);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderTypeDto> Create(ProviderTypeDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">SocialGroup entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderTypeDto> Update(ProviderTypeDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">SocialGroup key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}