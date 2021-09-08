using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private readonly IEntityRepository<Child> repository;
        private readonly ILogger<ChildService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the Child entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ChildService(IEntityRepository<Child> repository, ILogger<ChildService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChildDto> Create(ChildDto dto)
        {
            logger.LogInformation("Child creating was started.");

            this.Check(dto);

            var child = dto.ToDomain();

            var newChild = await repository.Create(child).ConfigureAwait(false);

            logger.LogInformation($"Child with Id = {newChild?.Id} created successfully.");

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDto>> GetAll()
        {
            logger.LogInformation("Getting all Children started.");

            var children = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!children.Any()
                ? "Child table is empty."
                : $"All {children.Count()} records were successfully received from the Child table");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDto> GetById(long id)
        {
            logger.LogInformation($"Getting Child by Id started. Looking Id = {id}.");

            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Child with Id = {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDto> GetByIdWithDetails(long id)
        {
            logger.LogInformation($"Getting Child by Id with details started. Looking ChildId = {id}.");

            Expression<Func<Child, bool>> filter = child => child.Id == id;

            var children =
                await this.repository.GetByFilter(filter, "Parent,SocialGroup").ConfigureAwait(false);

            logger.LogInformation($"Successfully got Child details with Id = {id}.");

            return await Task.Run(() => children.FirstOrDefault().ToModel()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDto>> GetAllByParent(long id, string userId)
        {
            logger.LogInformation($"Getting Child's by Parent started. Looking ParentId = {id}.");

            var children = await repository.GetByFilter(x => x.ParentId == id && x.Parent.UserId == userId).ConfigureAwait(false);

            logger.LogInformation(!children.Any()
                ? $"There aren't Children for Parent with Id = {id}."
                : $"All {children.Count()} records were successfully received from the Children table");

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDto> Update(ChildDto dto)
        {
            logger.LogInformation($"Updating Children with Id = {dto?.Id} started.");
            this.Check(dto);

            try
            {
                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"Children with Id = {child?.Id} updated succesfully.");

                return child.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Children with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.LogInformation($"Deleting Children with Id = {id} started.");

            var entity = new Child { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Children with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Children with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        private void Check(ChildDto dto)
        {
            if (dto == null)
            {
                logger.LogInformation("Child creating failed. Child is null.");
                throw new ArgumentNullException(nameof(dto), localizer["Child is null."]);
            }

            if (dto.DateOfBirth > DateTime.Now)
            {
                logger.LogInformation($"Child creating failed. Invalid Date of birth - {dto.DateOfBirth}.");
                throw new ArgumentException(localizer["Invalid Date of birth."]);
            }

            if (dto.FirstName.Length == 0)
            {
                logger.LogInformation("Updating failed. Empty firstname.");
                throw new ArgumentException(localizer["Empty firstname."], nameof(dto));
            }

            if (dto.LastName.Length == 0)
            {
                logger.LogInformation("Updating failed. Empty lastname.");
                throw new ArgumentException(localizer["Empty lastname."], nameof(dto));
            }

            if (dto.MiddleName.Length == 0)
            {
                logger.LogInformation("Updating failed. Empty patronymic.");
                throw new ArgumentException(localizer["Empty patronymic."], nameof(dto));
            }
        }
    }
}