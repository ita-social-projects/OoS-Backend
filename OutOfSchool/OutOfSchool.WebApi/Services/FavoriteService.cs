using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Favorite entity.
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IEntityRepository<Favorite> favoriteRepository;
        private readonly IWorkshopService workshopService;
        private readonly ILogger<FavoriteService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        public FavoriteService(
            IEntityRepository<Favorite> favoriteRepository,
            ILogger<FavoriteService> logger,
            IStringLocalizer<SharedResource> localizer,
            IWorkshopService worshopService)
        {
            this.favoriteRepository = favoriteRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.workshopService = worshopService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FavoriteDto>> GetAll()
        {
            logger.LogInformation("Getting all Favorites started.");

            var favorites = await favoriteRepository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!favorites.Any()
                ? "Favorites table is empty."
                : $"All {favorites.Count()} records were successfully received from the Favorites table");

            return favorites.Select(favorite => favorite.ToModel()).ToList();
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

            return favorite.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FavoriteDto>> GetAllByUser(string userId)
        {
            logger.LogInformation($"Getting Favorites by User started. Looking UserId = {userId}.");

            var favorites = await favoriteRepository.GetByFilter(x => x.UserId == userId).ConfigureAwait(false);

            logger.LogInformation(!favorites.Any()
                ? $"There aren't Favorites for User with Id = {userId}."
                : $"All {favorites.Count()} records were successfully received from the Favorites table");

            return favorites.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopCard>> GetFavoriteWorkshopsByUser(string userId, OffsetFilter offsetFilter)
        {
            logger.LogInformation($"Getting Favorites by User started. Looking UserId = {userId}.");

            var favorites = await favoriteRepository.Get<int>(where: x => x.UserId == userId).Select(x => x.WorkshopId).ToListAsync().ConfigureAwait(false);

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

            var favorite = dto.ToDomain();

            var newFavorite = await favoriteRepository.Create(favorite).ConfigureAwait(false);

            logger.LogInformation($"Favorite with Id = {newFavorite?.Id} created successfully.");

            return newFavorite.ToModel();
        }

        /// <inheritdoc/>
        public async Task<FavoriteDto> Update(FavoriteDto dto)
        {
            logger.LogInformation($"Updating Favorite with Id = {dto?.Id} started.");

            try
            {
                var favorite = await favoriteRepository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"Favorite with Id = {favorite?.Id} updated succesfully.");

                return favorite.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Favorite with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
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

        private List<WorkshopCard> DtoModelsToWorkshopCards(IEnumerable<WorkshopDTO> source)
        {
            List<WorkshopCard> workshopCards = new List<WorkshopCard>();
            foreach (var item in source)
            {
                workshopCards.Add(item.ToCardDto());
            }

            return workshopCards;
        }
    }
}
