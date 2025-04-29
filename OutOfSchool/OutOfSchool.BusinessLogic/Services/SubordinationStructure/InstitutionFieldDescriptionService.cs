using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.SubordinationStructure;

/// <summary>
/// Initializes a new instance of the <see cref="InstitutionFieldDescriptionService"/> class.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="cache">Redis cache service.</param>
public class InstitutionFieldDescriptionService(
    ISensitiveEntityRepositorySoftDeleted<InstitutionFieldDescription> repository,
    ILogger<InstitutionFieldDescriptionService> logger,
    ICacheService cache
) : IInstitutionFieldDescriptionService
{
    /// <inheritdoc/>
    public async Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionId(Guid id)
    {
        logger.LogInformation("Getting all entities InstitutionFieldDescription by Institution id started.");

        var cacheKey = $"InstitutionFieldDescriptionService_GetByInstitutionId_{id}";

        var institutionFieldDescriptions = await cache.GetOrAddAsync(cacheKey, () =>
            GetByInstitutionIdFromDatabase(id)).ConfigureAwait(false);

        return institutionFieldDescriptions;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionIdFromDatabase(Guid id)
    {
        var institutionFieldDescriptions = await repository.GetByFilter(i => i.InstitutionId == id).ConfigureAwait(false);

        logger.LogInformation(!institutionFieldDescriptions.Any()
            ? $"There is no desriptions in InstitutionFieldDescription table for id = {id}."
            : $"{institutionFieldDescriptions.Count()} records were successfully received from the InstitutionFieldDescription table for id = {id}.");

        return institutionFieldDescriptions.ToDto();
    }
}