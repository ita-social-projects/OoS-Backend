using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private readonly IEntityRepository<Child> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the Child entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ChildService(IEntityRepository<Child> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO dto)
        {
            logger.Information("Child creating was started.");

            this.Check(dto);

            var child = dto.ToDomain();

            var newChild = await repository.Create(child).ConfigureAwait(false);

            logger.Information("Child created successfully.");

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            logger.Information("Process of getting all Children started.");

            var children = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!children.Any()
                ? "Child table is empty."
                : "Successfully got all records from the Child table.");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            logger.Information("Process of getting Child by id started.");

            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Child with id = {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetByIdWithDetails(long id)
        {
            logger.Information("Process of getting child's details was launched.");

            Expression<Func<Child, bool>> filter = child => child.Id == id;

            var children =
                await this.repository.GetByFilter(filter, "Parent,SocialGroup").ConfigureAwait(false);

            logger.Information("Child details successfully retrieved.");

            return await Task.Run(() => children.FirstOrDefault().ToModel()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAllByParent(long id)
        {
            var children = await repository.GetByFilter(x => x.ParentId == id).ConfigureAwait(false);

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO dto)
        {
            logger.Information("Child updating was launched.");
            this.Check(dto);

            try
            {
                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Child successfully updated.");

                return child.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Child in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Child deleting was launched.");

            var entity = new Child { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Child successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Child in the Db with such an id.");
                throw;
            }
        }

        private void Check(ChildDTO dto)
        {
            if (dto == null)
            {
                logger.Information("Child creating failed. Child was null.");
                throw new ArgumentNullException(nameof(dto), "Child was null.");
            }

            if (dto.DateOfBirth > DateTime.Now)
            {
                logger.Information("Child creating failed. Invalid Date of birth.");
                throw new ArgumentException("Invalid Date of birth.");
            }

            if (dto.FirstName.Length == 0)
            {
                logger.Information("Updating failed. Empty firstname.");
                throw new ArgumentException("Empty firstname.", nameof(dto));
            }

            if (dto.LastName.Length == 0)
            {
                logger.Information("Updating failed. Empty lastname.");
                throw new ArgumentException("Empty lastname.", nameof(dto));
            }

            if (dto.MiddleName.Length == 0)
            {
                logger.Information("Updating failed. Empty patronymic.");
                throw new ArgumentException("Empty patronymic.", nameof(dto));
            }
        }
    }
}