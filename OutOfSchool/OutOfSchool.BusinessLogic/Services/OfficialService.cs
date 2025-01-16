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
    private readonly ILogger<OfficialDto> logger;
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
        ILogger<OfficialDto> logger,
        IMapper mapper
        )
    {
        this.officialRepository = officialRepository ?? throw new ArgumentNullException(nameof(officialRepository));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<SearchResult<OfficialDto>> GetByFilter(Guid providerId, OfficialFilter filter)
    {
        await providerService.HasProviderRights(providerId);

        logger.LogDebug("Getting Officials by filter started.");

        filter ??= new OfficialFilter();
        var predicate = BuildPredicate(filter);
        int count = await officialRepository.Count(predicate).ConfigureAwait(false);

        var officials = await officialRepository
            .Get(
             skip: filter.From,
             take: filter.Size,
             includeProperties: "Position, Individual",
             whereExpression: predicate
            ).AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the Officials table", officials.Count());

        var result = new SearchResult<OfficialDto>
        {
            Entities = mapper.Map<List<OfficialDto>>(officials),
            TotalAmount = count
        };

        return result;
    }

    private Expression<Func<Official, bool>> BuildPredicate(OfficialFilter filter)
    {
        var predicate = PredicateBuilder.True<Official>();

        if (!string.IsNullOrEmpty(filter.IndividualFirstName))
        {
            predicate = predicate.And(o => o.Individual.FirstName.Contains(filter.IndividualFirstName));
        }

        if (!string.IsNullOrEmpty(filter.IndividualMiddleName))
        {
            predicate = predicate.And(o => o.Individual.MiddleName.Contains(filter.IndividualMiddleName));
        }

        if (!string.IsNullOrEmpty(filter.IndividualLastName))
        {
            predicate = predicate.And(o => o.Individual.LastName.Contains(filter.IndividualLastName));
        }

        if (!string.IsNullOrEmpty(filter.IndividualRnokpp))
        {
            predicate = predicate.And(o => o.Individual.Rnokpp.Contains(filter.IndividualRnokpp));
        }

        predicate = predicate.And(s => !s.IsDeleted);

        return predicate;
    }
}
