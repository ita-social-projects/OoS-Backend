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
    /// Service with business logic for ParentController.
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly IParentRepository repositoryParent;
        private readonly IEntityRepository<User> repositoryUser;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentService"/> class.
        /// </summary>
        /// <param name="repositoryParent">Repository for parent entity.</param>
        /// <param name="repositoryUser">Repository for user entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ParentService(IParentRepository repositoryParent, IEntityRepository<User> repositoryUser, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repositoryParent = repositoryParent;
            this.repositoryUser = repositoryUser;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Create(ParentDTO dto)
        {
            logger.Information("Parent creating was started");

            Func<Task<Parent>> operation = async () => await repositoryParent.Create(dto.ToDomain()).ConfigureAwait(false);

            var newParent = await repositoryParent.RunInTransaction(operation).ConfigureAwait(false);

            logger.Information($"Parent with Id = {newParent?.Id} created successfully.");

            return newParent.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Parent with Id = {id} started.");

            var entity = new Parent() { Id = id };

            try
            {
                await repositoryParent.Delete(entity).ConfigureAwait(false);

                logger.Information($"Parent with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Parent with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ParentDTO>> GetAll()
        {
            logger.Information("Getting all Parents started.");

            var parents = await this.repositoryParent.GetAll().ConfigureAwait(false);

            logger.Information(!parents.Any()
                ? "Parent table is empty."
                : $"All {parents.Count()} records were successfully received from the Parent table");

            return parents.Select(parent => parent.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetByUserId(string id)
        {
            logger.Information($"Getting Parent by UserId started. Looking UserId is {id}.");

            Expression<Func<Parent, bool>> filter = p => p.UserId == id;

            var parents = await repositoryParent.GetByFilter(filter).ConfigureAwait(false);

            if (!parents.Any())
            {
                throw new ArgumentException(localizer["There is no Parent in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got a Parent with UserId = {id}.");

            return parents.FirstOrDefault().ToModel();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetById(long id)
        {
            logger.Information($"Getting Parent by Id started. Looking Id = {id}.");

            var parent = await repositoryParent.GetById((int)id).ConfigureAwait(false);

            if (parent == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Parent with Id = {id}.");

            return parent.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ShortUserDto> Update(ShortUserDto dto)
        {
            logger.Information($"Updating Parent with User Id = {dto?.Id} started.");

            try
            {
                Expression<Func<User, bool>> filter = p => p.Id == dto.Id;

                var users = repositoryUser.GetByFilterNoTracking(filter);

                var updatedUser = await repositoryUser.Update(dto.ToDomain(users.FirstOrDefault())).ConfigureAwait(false);

                logger.Information($"User with Id = {updatedUser?.Id} updated succesfully.");

                return updatedUser.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. User with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }
    }
}
