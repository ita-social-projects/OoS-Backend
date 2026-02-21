using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.SubordinationStructure;

/// <summary>
/// Initializes a new instance of the <see cref="InstitutionService"/> class.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="cache">Redis cache service.</param>
/// <param name="currentUserService">Service for manage current user.</param>
/// <param name="ministryAdminService">Service for manage ministry admin</param>
/// <param name="regionAdminService">Service for managing region admin rigths.</param>
public class InstitutionService(
    ISensitiveEntityRepositorySoftDeleted<Institution> repository,
    ILogger<InstitutionService> logger,
    IMultiLayerCacheService cache,
    ICurrentUserService currentUserService,
    IMinistryAdminService ministryAdminService,
    IRegionAdminService regionAdminService
) : IInstitutionService
{
    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAll(bool filterNonGovernment)
    {
        logger.LogInformation("Getting all Institutions started");

        var cacheKey = "InstitutionService_GetAll";

        var institutions = await cache.GetOrAddAsync(cacheKey, GetAllFromDatabase);

        var filterPredicate = await PredicateBuild(filterNonGovernment);
        institutions = institutions.Where(filterPredicate).ToList();

        logger.LogInformation(institutions.Count == 0
            ? "Institution table is empty."
            : $"All {institutions.Count} records were successfully received from the Institution table");

        return institutions;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAllFromDatabase()
    {
        var institutions = await repository.GetAll().ConfigureAwait(false);
        return institutions.ToDto();
    }

    private async Task<Func<InstitutionDto, bool>> PredicateBuild(bool filterNonGovernment)
    {
        var predicate = PredicateBuilder.True<InstitutionDto>();

        predicate = filterNonGovernment ? predicate.And(x => x.IsGovernment) : predicate;

        var filteredInstitutionId = Guid.Empty;

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            filteredInstitutionId = ministryAdmin.InstitutionId;
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            filteredInstitutionId = regionAdmin.InstitutionId;
        }

        predicate = filteredInstitutionId.Equals(Guid.Empty) ? predicate : predicate.And(i => i.Id == filteredInstitutionId);

        return predicate.Compile();
    }
}