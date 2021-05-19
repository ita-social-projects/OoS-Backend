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
    /// SubsubcategoryService.
    /// </summary>
    public class SubsubcategoryService : ISubsubcategoryService
    {
        private readonly ISubsubcategoryRepository repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsubcategoryService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public SubsubcategoryService(ISubsubcategoryRepository entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SubsubcategoryDTO> Create(SubsubcategoryDTO dto)
        {
            logger.Information("Subsubategory creating was started.");

            var category = dto.ToDomain();

            ModelValidation(dto);

            var newCategory = await repository.Create(category).ConfigureAwait(false);

            logger.Information($"Subsubcategory with Id = {newCategory.Id} created successfully.");

            return newCategory.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Subsubcategory with Id = {id} started.");

            var entity = new Subsubcategory() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Subsubcategory with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Subsubcategory with such Id - {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubsubcategoryDTO>> GetAll()
        {
            logger.Information("Getting all Subsubcategories started.");

            var categories = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? "Subsubcategory table is empty."
                : $"From the Subsubcategory table were successfully received all {categories.Count()} records.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubsubcategoryDTO> GetById(long id)
        {
            logger.Information($"Getting Subsubcategory by Id started. Looking Id is {id}.");

            var category = await repository.GetById((int)id).ConfigureAwait(false);

            if (category == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Subsubcategory with Id = {id}.");

            return category.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubsubcategoryDTO>> GetBySubcategoryId(long id)
        {
            logger.Information($"Getting Subsubcategory by SubcategoryId started. Looking SubcategoryId is {id}.");

            IdValidation(id);

            var categories = await this.repository.Get<int>(where: x => x.SubcategoryId == id).ToListAsync().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? $"There aren't Subsubcategories for Subcategory with Id = {id}."
                : $"From Subsubcategory table were successfully received {categories.Count} records.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubsubcategoryDTO> Update(SubsubcategoryDTO dto)
        {
            logger.Information($"Updating Subsubcategory with Id = {dto.Id} started.");

            try
            {
                var category = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Subsubcategory with Id = {category.Id} updated succesfully.");

                return category.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Subsubcategory with Id - {dto.Id} doesn't exist in the system.");
                throw;
            }
        }

        private void ModelValidation(SubsubcategoryDTO dto)
        {
            if (repository.Get<int>(where: x => x.Title == dto.Title).Any())
            {
                throw new ArgumentException(localizer["There is already a subsubcategory with such a data."]);
            }
        }

        private void IdValidation(long id)
        {
            if (!repository.SubcategoryExists(id))
            {
                throw new ArgumentException(localizer["There is no subcategory with such id."]);
            }
        }
    }
}
