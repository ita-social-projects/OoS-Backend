using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Initializes a new instance of the <see cref="ProviderTypeService"/> class.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="mapper">Mapper.</param>
public class ProviderTypeService(
    IEntityRepository<long, ProviderType> repository,
    ILogger<ProviderTypeService> logger,
    IStringLocalizer<SharedResource> localizer
) : IProviderTypeService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<ProviderTypeDto>> GetAll()
    {
        logger.LogInformation("Getting all Provider types started.");

        var providerTypes = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!providerTypes.Any()
            ? "Provider types table is empty."
            : $"All {providerTypes.Count()} records were successfully received from the Provider types table");

        return providerTypes.ToDto();
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> GetById(long id)
    {
        logger.LogInformation($"Getting Provider type by Id started. Looking Id = {id}.");

        var providerType = await repository.GetById(id).ConfigureAwait(false);

        if (providerType == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Provider type with Id = {id}.");

        return providerType.ToDto();
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> Create(ProviderTypeDto dto)
    {
        logger.LogInformation("Provider type creating was started.");

        var newProviderType = await repository.Create(dto.ToModel()).ConfigureAwait(false);

        logger.LogInformation($"Provider type with Id = {newProviderType?.Id} created successfully.");

        return newProviderType.ToDto();
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> Update(ProviderTypeDto dto)
    {
        logger.LogInformation($"Updating Provider type with Id = {dto?.Id} started.");

        try
        {
            var providerType = await repository.Update(dto.ToModel()).ConfigureAwait(false);

            logger.LogInformation($"Provider type with Id = {providerType?.Id} updated succesfully.");

            return providerType.ToDto();
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. Provider type with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Provider type with Id = {id} started.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Provider type with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"Provider type with Id = {id} succesfully deleted.");
    }
}