using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Favorite entity.
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <returns>List of all Favorites.</returns>
    Task<IEnumerable<FavoriteDto>> GetAll();

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Favorite.</returns>
    Task<FavoriteDto> GetById(long id);

    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <returns>List of all Favorites by User.</returns>
    Task<IEnumerable<FavoriteDto>> GetAllByUser(string userId);

    /// <summary>
    /// Get all user's favorite workshops.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <param name="offsetFilter">Filter to get spesified portion of entities.</param>
    /// <returns>List of all Favorites by User.</returns>
    Task<SearchResult<WorkshopCard>> GetFavoriteWorkshopsByUser(string userId, OffsetFilter offsetFilter);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Favorite entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<FavoriteDto> Create(FavoriteDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">Favorite entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<FavoriteDto> Update(FavoriteDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Favorite key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}