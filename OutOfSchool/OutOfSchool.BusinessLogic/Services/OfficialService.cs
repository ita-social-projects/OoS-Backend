using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Initializes a new instance of the <see cref="OfficialService"/> class.
/// </summary>
/// <param name="officialRepository">Repository for Officials.</param>
/// <param name="currentUserService">Service for current user.</param>
/// <param name="logger">Logger.</param>
public class OfficialService(
    IOfficialRepository officialRepository,
    IOfficialChangesLogService officialChangesLogService,
    ICurrentUserService currentUserService,
    ILogger<OfficialService> logger
) : IOfficialService
{
    /// <inheritdoc/>
    public async Task<SearchResult<OfficialDto>> GetByFilter(Guid providerId, SearchStringFilter filter)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        logger.LogDebug("Getting Officials by filter started.");

        filter ??= new SearchStringFilter();
        var predicate = BuildPredicate(filter);
        predicate = predicate.And(p => p.Position.ProviderId == providerId);

        var count = await officialRepository.Count(predicate).ConfigureAwait(false);

        var officials = await officialRepository
            .Get(
             skip: filter.From,
             take: filter.Size,
             whereExpression: predicate)
            .Include(o => o.Position)
            .Include(o => o.Individual)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the Officials table", officials.Count);

        var result = new SearchResult<OfficialDto>
        {
            Entities = officials.ToDto(),
            TotalAmount = count
        };

        return result;
    }

    private static Expression<Func<Official, bool>> BuildPredicate(SearchStringFilter filter)
    {
        var predicate = PredicateBuilder.True<Official>();

        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            predicate = predicate.And(o => o.Individual.FirstName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase)
                || o.Individual.MiddleName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase)
                || o.Individual.LastName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase)
                || o.Individual.Rnokpp.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase)
                || o.Position.FullName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
        }

        predicate = predicate.And(o => !o.IsDeleted);

        return predicate;
    }
}
