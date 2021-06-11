using System;
using System.Collections.Generic;
using System.Data;
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
    /// Implements the interface with functionality for User entity.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IEntityRepository<User> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public UserService(IEntityRepository<User> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        public async Task<IEnumerable<ShortUserDto>> GetAll()
        {
            logger.Information("Getting all Users started.");

            var users = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!users.Any()
                ? "User table is empty."
                : $"All {users.Count()} records were successfully received from the User table");

            return users.Select(user => user.ToModel()).ToList();
        }

        public async Task<ShortUserDto> GetById(string id)
        {
            logger.Information($"Getting User by Id started. Looking Id = {id}.");

            Expression<Func<User, bool>> filter = p => p.Id == id;

            var users = await repository.GetByFilter(filter).ConfigureAwait(false);
         
            if (!users.Any())
            {
                throw new ArgumentException(localizer["There is no User in the Db with such an id"], nameof(id));
            }

            logger.Information($"Successfully got an User with Id = {id}.");

            return users.FirstOrDefault().ToModel();
        }

        public async Task<ShortUserDto> Update(ShortUserDto dto)
        {
            logger.Information($"Updating User with Id = {dto?.Id} started.");
            
            try
            {
                Expression<Func<User, bool>> filter = p => p.Id == dto.Id;

                var users = await repository.GetByFilter(filter).ConfigureAwait(false);

                var updatedUser = await repository.Update(dto.ToDomain(users.FirstOrDefault())).ConfigureAwait(false);

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
