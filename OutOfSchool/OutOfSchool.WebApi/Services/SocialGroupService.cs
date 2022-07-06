using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for SocialGroup entity.
/// </summary>
public class SocialGroupService : ISocialGroupService
{
    private readonly IEntityRepository<SocialGroup> repository;
    private readonly ILogger<SocialGroupService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialGroupService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    public SocialGroupService(IEntityRepository<SocialGroup> repository, ILogger<SocialGroupService> logger, IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        this.repository = repository;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SocialGroupDto>> GetAll()
    {
        logger.LogInformation("Getting all Social Groups started.");

        var socialGroups = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!socialGroups.Any()
            ? "SocialGroup table is empty."
            : $"All {socialGroups.Count()} records were successfully received from the SocialGroup table");

        return socialGroups.Select(socialGroup => socialGroup.ToModel()).ToList();
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> GetById(long id)
    {
        logger.LogInformation($"Getting SocialGroup by Id started. Looking Id = {id}.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a SocialGroup with Id = {id}.");

        return socialGroup.ToModel();
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> Create(SocialGroupDto dto)
    {
        logger.LogInformation("SocialGroup creating was started.");

        var socialGroup = dto.ToDomain();

        var newSocialGroup = await repository.Create(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {newSocialGroup?.Id} created successfully.");

        return newSocialGroup.ToModel();
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> Update(SocialGroupDto dto)
    {
        logger.LogInformation($"Updating SocialGroup with Id = {dto?.Id} started.");

        try
        {
            var socialGroup = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

            logger.LogInformation($"SocialGroup with Id = {socialGroup?.Id} updated succesfully.");

            return socialGroup.ToModel();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. SocialGroup with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting SocialGroup with Id = {id} started.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"SocialGroup with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {id} succesfully deleted.");
    }
}