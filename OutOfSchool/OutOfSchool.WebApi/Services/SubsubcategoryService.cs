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
    /// Service with business logic for SubsubcategoryController.
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
            logger.Information("Subsubategory creating was started");

            var category = dto.ToDomain();

            ModelValidation(dto);

            var newCategory = await repository.Create(category).ConfigureAwait(false);

            logger.Information("Subsubcategory created successfully.");

            return newCategory.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Subsubcategoty deleting was launched.");

            var entity = new Subsubcategory() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Subsubcategory succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed.There is no subsubcategory in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubsubcategoryDTO>> GetAll()
        {
            logger.Information("Process of getting all Subsubcategories started.");

            var categories = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? "Subsubcategory table is empty."
                : "Successfully got all records from the Subsubcategory table.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubsubcategoryDTO> GetById(long id)
        {
            logger.Information("Process of getting Subsubcategory by id started.");

            var category = await repository.GetById((int)id).ConfigureAwait(false);

            if (category == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfuly got a subsubcategory with id = {id}.");

            return category.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SubsubcategoryDTO>> GetBySubcategoryId(long id)
        {
            logger.Information("Process of getting all Subsubcategories started.");

            IdValidation(id);

            var categories = await this.repository.Get<int>(where: x => x.SubcategoryId == id).ToListAsync().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? "Subsubcategory table is empty."
                : "Successfully got all records from the Subsubcategory table.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<SubsubcategoryDTO> Update(SubsubcategoryDTO dto)
        {
            logger.Information("Subsubcategory updating was launched.");

            try
            {
                var category = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Subsubcategory succesfully updated.");

                return category.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no subsubcategory in the Db with such an id.");
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
