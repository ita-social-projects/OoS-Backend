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
    /// CategoryService.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IEntityRepository<Category> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public CategoryService(IEntityRepository<Category> entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CategoryDTO> Create(CategoryDTO dto)
        {
            logger.Information("Category creating was started");

            var category = dto.ToDomain();

            CategoryValidation(dto);

            var newCategory = await repository.Create(category).ConfigureAwait(false);

            logger.Information("Category created successfully.");

            return newCategory.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Categoty deleting was launched.");

            var entity = new Category() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Category succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed.There is no category in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryDTO>> GetAll()
        {
            logger.Information("Process of getting all Categories started.");

            var categories = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!categories.Any()
                ? "Category table is empty."
                : "Successfully got all records from the Category table.");

            return categories.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<CategoryDTO> GetById(long id)
        {
            logger.Information("Process of getting Category by id started.");

            var category = await repository.GetById((int)id).ConfigureAwait(false);

            if (category == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfuly got a category with id = {id}.");

            return category.ToModel();
        }

        /// <inheritdoc/>
        public async Task<CategoryDTO> Update(CategoryDTO dto)
        {
            logger.Information("Category updating was launched.");

            try
            {
                var category = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Category succesfully updated.");

                return category.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no category in the Db with such an id.");
                throw;
            }
        }

        private void CategoryValidation(CategoryDTO dto)
        {
            if (repository.Get<int>(where: x => x.Title == dto.Title).Any())
            {
                throw new ArgumentException(localizer["There is already a category with such a data."]);
            }
        }
    }
}
