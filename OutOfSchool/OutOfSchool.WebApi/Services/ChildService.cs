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

            logger.Information($"Child with Id = {newChild?.Id} created successfully.");

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            logger.Information("Getting all Children started.");

            var children = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!children.Any()
                ? "Child table is empty."
                : $"From the Child table were successfully received all {children.Count()} records.");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            logger.Information($"Getting Child by Id started. Looking Id is {id}.");

            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Child with Id = {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetByIdWithDetails(long id)
        {
            logger.Information($"Getting Child by Id with details started. Looking CategoryId is {id}.");

            Expression<Func<Child, bool>> filter = child => child.Id == id;

            var children =
                await this.repository.GetByFilter(filter, "Parent,SocialGroup").ConfigureAwait(false);

            logger.Information($"Successfully got Child details with Id = {id}.");

            return await Task.Run(() => children.FirstOrDefault().ToModel()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAllByParent(long id)
        {
            logger.Information($"Getting Child's by Parent started. Looking ParentId is {id}.");

            var children = await repository.GetByFilter(x => x.ParentId == id).ConfigureAwait(false);

            logger.Information(!children.Any()
                ? $"There aren't Children for Parent with Id = {id}."
                : $"From Children table were successfully received {children.Count()} records.");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO dto)
        {
            logger.Information($"Updating Children with Id = {dto?.Id} started.");
            this.Check(dto);

            try
            {
                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Children with Id = {child?.Id} updated succesfully.");

                return child.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Children with Id - {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Children with Id = {id} started.");

            var entity = new Child { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Children with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Children with Id - {id} doesn't exist in the system.");
                throw;
            }
        }

        private void Check(ChildDTO dto)
        {
            if (dto == null)
            {
                logger.Information("Child creating failed. Child was null.");
                throw new ArgumentNullException(nameof(dto), localizer["Child was null."]);
            }

            if (dto.DateOfBirth > DateTime.Now)
            {
                logger.Information($"Child creating failed. Invalid Date of birth - {dto.DateOfBirth}.");
                throw new ArgumentException(localizer["Invalid Date of birth."]);
            }

            if (dto.FirstName.Length == 0)
            {
                logger.Information("Updating failed. Empty firstname.");
                throw new ArgumentException(localizer["Empty firstname."], nameof(dto));
            }

            if (dto.LastName.Length == 0)
            {
                logger.Information("Updating failed. Empty lastname.");
                throw new ArgumentException(localizer["Empty lastname."], nameof(dto));
            }

            if (dto.MiddleName.Length == 0)
            {
                logger.Information("Updating failed. Empty patronymic.");
                throw new ArgumentException(localizer["Empty patronymic."], nameof(dto));
            }
        }
    }
}