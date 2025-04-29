using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Favorite entity.
/// </summary>
public class FavoriteService(
    IEntityRepositorySoftDeleted<long, Favorite> favoriteRepository,
    ILogger<FavoriteService> logger,
    IStringLocalizer<SharedResource> localizer,
    IWorkshopService workshopService
) : IFavoriteService
{

    /// <inheritdoc/>
    public async Task<IEnumerable<FavoriteDto>> GetAll()
    {
        logger.LogInformation("Getting all Favorites started.");

        var favorites = await favoriteRepository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!favorites.Any()
            ? "Favorites table is empty."
            : $"All {favorites.Count()} records were successfully received from the Favorites table");

        return favorites.ToDto();
    }

    /// <inheritdoc/>
    public async Task<FavoriteDto> GetById(long id)
    {
        logger.LogInformation($"Getting Favorite by Id started. Looking Id = {id}.");

        var favorite = await favoriteRepository.GetById(id).ConfigureAwait(false);

        if (favorite == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Favorite with Id = {id}.");

        return favorite.ToDto();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FavoriteDto>> GetAllByUser(string userId)
    {
        logger.LogInformation("Getting Favorites by User started. Looking UserId = {UserId}", userId);

        var favoritesQuery = await favoriteRepository
            .GetByFilter(x => x.UserId == userId && Provider.ValidProviderStatuses.Contains(x.Workshop.Provider.Status))
            .ConfigureAwait(false);

        var favorites = favoritesQuery.ToList();

        logger.LogInformation(
            "All {Count} records were successfully received from the Favorites table for User with Id = {UserId}",
            favorites.Count,
            userId);

        return favorites.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopCard>> GetFavoriteWorkshopsByUser(string userId, OffsetFilter offsetFilter)
    {
        logger.LogInformation($"Getting Favorites by User started. Looking UserId = {userId}.");

        var favorites = await favoriteRepository
            .Get(whereExpression: x => x.UserId == userId && Provider.ValidProviderStatuses.Contains(x.Workshop.Provider.Status))
            .Select(x => x.WorkshopId)
            .ToListAsync()
            .ConfigureAwait(false);

        if (!favorites.Any())
        {
            return new SearchResult<WorkshopCard>();
        }

        if (offsetFilter is null)
        {
            offsetFilter = new OffsetFilter();
        }

        var filter = new WorkshopFilter() { Ids = favorites, Size = offsetFilter.Size, From = offsetFilter.From };

        var workshops = await workshopService.GetByFilter(filter).ConfigureAwait(false);

        logger.LogInformation(!workshops.Entities.Any()
            ? $"There aren't Favorites for User with Id = {userId}."
            : $"All {workshops.TotalAmount} records were successfully received from the Favorites table");

        return new SearchResult<WorkshopCard>() { TotalAmount = workshops.TotalAmount, Entities = workshops.Entities };
    }

    /// <inheritdoc/>
    public async Task<FavoriteDto> Create(FavoriteDto dto)
    {
        logger.LogInformation("Favorite creating was started.");

        var newFavorite = await favoriteRepository.Create(dto.ToModel()).ConfigureAwait(false);

        logger.LogInformation($"Favorite with Id = {newFavorite?.Id} created successfully.");

        return newFavorite.ToDto();
    }

    /// <inheritdoc/>
    public async Task<FavoriteDto> Update(FavoriteDto dto)
    {
        logger.LogInformation($"Updating Favorite with Id = {dto?.Id} started.");

        ArgumentNullException.ThrowIfNull(dto);

        var favorite = await favoriteRepository.GetById(dto.Id).ConfigureAwait(false);

        if (favorite is null)
        {
            var message = $"Updating failed. Favorite with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        favorite = await favoriteRepository.Update(dto.SetToModel(favorite)).ConfigureAwait(false);

        logger.LogInformation($"Favorite with Id = {favorite?.Id} updated succesfully.");

        return favorite.ToDto();
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Favorite with Id = {id} started.");

        var favorite = await favoriteRepository.GetById(id).ConfigureAwait(false);

        if (favorite == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Favorite with Id = {id} doesn't exist in the system"]);
        }

        await favoriteRepository.Delete(favorite).ConfigureAwait(false);

        logger.LogInformation($"Favorite with Id = {id} succesfully deleted.");
    }
}