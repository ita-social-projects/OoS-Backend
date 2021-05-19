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
    /// SubcategoryService.
    /// </summary>
    public class SubcategoryService : ISubcategoryService
    {
        private readonly ISubcategoryRepository repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcategoryService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public SubcategoryService(ISubcategoryRepository entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SubcategoryDTO> Create(SubcategoryDTO dto)
        {
            logger.Information("Subcategory creating was started.");

            var category = dto.ToDomain();

            ModelValidation(dto);

            var newCategory = await repository.Create(category).ConfigureAwait(false);

            logger.Information($"Subcategory with Id = {newCategory.Id} created successfully.");

            return newCategory.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Subcategory with Id = {id} started.");

            var entity = new Subcategory() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Subcategory with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Subcategory with Id - {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubcategoryDTO>> GetAll()
        {
            logger.Information("Getting all Subcategories started.");

            var categories = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? "Subcategory table is empty."
                : $"From the Subcategory table were successfully received all {categories.Count()} records.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubcategoryDTO> GetById(long id)
        {
            logger.Information($"Getting Subcategory by Id started. Looking Id is {id}.");

            var category = await repository.GetById((int)id).ConfigureAwait(false);

            if (category == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Subcategory with Id = {id}.");

            return category.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubcategoryDTO>> GetByCategoryId(long id)
        {
            logger.Information($"Getting Subcategory by CategoryId started. Looking CategoryId is {id}.");

            IdValidation(id);

            var categories = await this.repository.Get<int>(where: x => x.CategoryId == id).ToListAsync().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? $"There aren't Subcategories for Category with Id = {id}."
                : $"From Subcategory table were successfully received {categories.Count} records.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubcategoryDTO> Update(SubcategoryDTO dto)
        {
            logger.Information($"Updating Subcategory with Id = {dto.Id} started.");

            ModelValidation(dto);

            try
            {
                var category = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Subcategory with Id = {category.Id} updated succesfully.");

                return category.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Subcategory with Id - {dto.Id} doesn't exist in the system.");
                throw;
            }
        }

        private void ModelValidation(SubcategoryDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentException(localizer["Object cannot be null."]);
            }

            if (!repository.CategoryExists(dto.CategoryId))
            {
                throw new ArgumentException(localizer["There is no category with such id."]);
            }

            if (repository.SameExists(dto.ToDomain()))
            {
                throw new ArgumentException(localizer["There is already a subcategory with such a data."]);
            }
        }

        private void IdValidation(long id)
        {
            if (!repository.CategoryExists(id))
            {
                throw new ArgumentException(localizer["There is no subcategory with such id."]);
            }
        }
    }
}
