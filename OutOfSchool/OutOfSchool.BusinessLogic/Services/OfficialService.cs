using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Repository.Base.Api;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services;
public class OfficialService : IOfficialService
{
    private readonly IEntityRepositorySoftDeleted<Guid, Official> officialRepository;
    private readonly IProviderService providerService;
    private readonly ILogger<OfficialService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfficialService"/> class.
    /// </summary>
    /// <param name="officialRepository">Repository for Officials.</param>
    /// <param name="providerService">Service for Provider.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public OfficialService(
        IEntityRepositorySoftDeleted<Guid, Official> officialRepository,
        IProviderService providerService,
        ILogger<OfficialService> logger,
        IMapper mapper
        )
    {
        this.officialRepository = officialRepository ?? throw new ArgumentNullException(nameof(officialRepository));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<SearchResult<OfficialDto>> GetByFilter(Guid providerId, SearchStringFilter filter)
    {
        await providerService.HasProviderRights(providerId);

        logger.LogDebug("Getting Officials by filter started.");

        filter ??= new SearchStringFilter();
        var predicate = BuildPredicate(filter);
        int count = await officialRepository.Count(predicate).ConfigureAwait(false);

        var officials = await officialRepository
            .Get(
             skip: filter.From,
             take: filter.Size,
             includeProperties: "Position,Individual",
             whereExpression: predicate
            ).AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the Officials table", officials.Count);

        var result = new SearchResult<OfficialDto>
        {
            Entities = mapper.Map<List<OfficialDto>>(officials),
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
