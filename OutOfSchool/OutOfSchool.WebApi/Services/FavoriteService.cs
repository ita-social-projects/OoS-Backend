using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Favorite entity.
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IEntityRepository<Favorite> repository;
        private readonly IWorkshopService workshopService;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        public FavoriteService(IEntityRepository<Favorite> repository, ILogger logger, IStringLocalizer<SharedResource> localizer, IWorkshopService worshopService)
        {
            this.repository = repository;
            this.logger = logger;
            this.localizer = localizer;
            this.workshopService = worshopService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FavoriteDto>> GetAll()
        {
            logger.Information("Getting all Favorites started.");

            var favorites = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!favorites.Any()
                ? "Favorites table is empty."
                : $"All {favorites.Count()} records were successfully received from the Favorites table");

            return favorites.Select(favorite => favorite.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<FavoriteDto> GetById(long id)
        {
            logger.Information($"Getting Favorite by Id started. Looking Id = {id}.");

            var favorite = await repository.GetById(id).ConfigureAwait(false);

            if (favorite == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Favorite with Id = {id}.");

            return favorite.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FavoriteDto>> GetAllByUser(string userId)
        {
            logger.Information($"Getting Favorites by User started. Looking UserId = {userId}.");

            var favorites = await repository.GetByFilter(x => x.UserId == userId).ConfigureAwait(false);

            logger.Information(!favorites.Any()
                ? $"There aren't Favorites for User with Id = {userId}."
                : $"All {favorites.Count()} records were successfully received from the Favorites table");

            return favorites.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopCard>> GetFavoriteWorkshopsByUser(string userId, OffsetFilter offsetFilter)
        {
            logger.Information($"Getting Favorites by User started. Looking UserId = {userId}.");

            var favorites = await repository.Get<int>(where: x => x.UserId == userId).Select(x => x.WorkshopId).ToListAsync().ConfigureAwait(false);

            if (offsetFilter is null)
            {
                offsetFilter = new OffsetFilter();
            }

            var filter = new WorkshopFilter() { Ids = favorites, Size = offsetFilter.Size, From = offsetFilter.From };

            var workshops = await workshopService.GetByFilter(filter).ConfigureAwait(false);

            logger.Information(!workshops.Entities.Any()
                ? $"There aren't Favorites for User with Id = {userId}."
                : $"All {workshops.TotalAmount} records were successfully received from the Favorites table");

            return new SearchResult<WorkshopCard>() { TotalAmount = workshops.TotalAmount, Entities = DtoModelsToWorkshopCards(workshops.Entities) };
        }

        /// <inheritdoc/>
        public async Task<FavoriteDto> Create(FavoriteDto dto)
        {
            logger.Information("Favorite creating was started.");

            var favorite = dto.ToDomain();

            var newFavorite = await repository.Create(favorite).ConfigureAwait(false);

            logger.Information($"Favorite with Id = {newFavorite?.Id} created successfully.");

            return newFavorite.ToModel();
        }

        /// <inheritdoc/>
        public async Task<FavoriteDto> Update(FavoriteDto dto)
        {
            logger.Information($"Updating Favorite with Id = {dto?.Id} started.");

            try
            {
                var favorite = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Favorite with Id = {favorite?.Id} updated succesfully.");

                return favorite.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Favorite with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Favorite with Id = {id} started.");

            var favorite = await repository.GetById(id).ConfigureAwait(false);

            if (favorite == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"Favorite with Id = {id} doesn't exist in the system"]);
            }

            await repository.Delete(favorite).ConfigureAwait(false);

            logger.Information($"Favorite with Id = {id} succesfully deleted.");
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
