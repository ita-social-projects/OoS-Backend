﻿using System.Data;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with functionality for User entity.
/// </summary>
public class UserService : IUserService
{
    private readonly IEntityRepositorySoftDeleted<string, User> repository;
    private readonly ILogger<UserService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public UserService(
        IEntityRepositorySoftDeleted<string, User> repository,
        ILogger<UserService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ShortUserDto>> GetAll()
    {
        logger.LogInformation("Getting all Users started.");

        var users = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!users.Any()
            ? "User table is empty."
            : $"All {users.Count()} records were successfully received from the User table");

        return users.Select(user => mapper.Map<ShortUserDto>(user)).ToList();
    }

    // TODO: use repository.GetById() method
    public async Task<ShortUserDto> GetById(string id)
    {
        logger.LogInformation($"Getting User by Id started. Looking Id = {id}.");

        Expression<Func<User, bool>> filter = p => p.Id == id;

        var users = await repository.GetByFilter(filter).ConfigureAwait(false);

        if (!users.Any())
        {
            throw new ArgumentException(localizer["There is no User in the Db with such an id"], nameof(id));
        }

        logger.LogInformation($"Successfully got an User with Id = {id}.");

        return mapper.Map<ShortUserDto>(users.First());
    }

    public async Task<ShortUserDto> Update(ShortUserDto dto)
    {
        logger.LogInformation($"Updating User with Id = {dto?.Id} started.");

        try
        {
            Expression<Func<User, bool>> filter = p => p.Id == dto.Id;

            var users = repository.GetByFilterNoTracking(filter);

            var updatedUser = await repository.Update(mapper.Map(dto, users.FirstOrDefault())).ConfigureAwait(false);

            logger.LogInformation($"User with Id = {updatedUser?.Id} updated succesfully.");

            return mapper.Map<ShortUserDto>(updatedUser);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. User with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    public async Task<bool> IsBlocked(string id)
    {
        logger.LogInformation("Checking if the User is blocked was started. Getting user by Id = {id}.", id);

        var user = await repository.GetById(id).ConfigureAwait(false);

        if (user is null)
        {
            throw new ArgumentException(localizer["There is no User in the Db with such an id"], nameof(id));
        }

        logger.LogInformation("Successfully got the User with Id = {id}.", id);

        return user.IsBlocked;
    }
}