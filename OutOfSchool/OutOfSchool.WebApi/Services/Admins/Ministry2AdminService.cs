using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;

namespace OutOfSchool.WebApi.Services.Admins;

public class Ministry2AdminService : BaseAdminService
{
    private const string IncludePropertiers = "Institution,User";

    private readonly ILogger<Ministry2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IInstitutionAdminRepository institutionAdminRepository;

    private readonly ExpressionConverter<InstitutionAdminBase, InstitutionAdmin> expressionConverter;
    private readonly ExpressionConverter<InstitutionAdmin, InstitutionAdminBase> expressionConverter2;

    public Ministry2AdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<Ministry2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IInstitutionAdminRepository institutionAdminRepository)
        : base(
            httpClientFactory,
            authorizationServerConfig,
            communicationConfig,
            logger,
            mapper,
            userService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(institutionAdminRepository);

        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.institutionAdminRepository = institutionAdminRepository;

        expressionConverter = new ExpressionConverter<InstitutionAdminBase, InstitutionAdmin>();
        expressionConverter2 = new ExpressionConverter<InstitutionAdmin, InstitutionAdminBase>();
    }

    protected override async Task<BaseAdminDto> GetById(string id) =>
        mapper.Map<Ministry2AdminDto>(await institutionAdminRepository.GetByIdAsync(id));

    protected override async Task<BaseAdminDto> GetByUserId(string userId) =>
        mapper.Map<Ministry2AdminDto>((await institutionAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Ministry2AdminFilter CreateEmptyFilter() => new Ministry2AdminFilter();

    protected override async Task<bool> IsUserHasRightsToGetAdminsByFilter(BaseAdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var admin = await GetByUserId(currentUserService.UserId) as Ministry2AdminDto;

            if ((filter as Ministry2AdminFilter).InstitutionId != admin.InstitutionId
                && (filter as Ministry2AdminFilter).InstitutionId != Guid.Empty)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task UpdateTheFilterWithTheAdminRestrictions(BaseAdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var admin = await GetByUserId(currentUserService.UserId) as Ministry2AdminDto;

            if ((filter as Ministry2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Ministry2AdminFilter).InstitutionId = admin.InstitutionId;
            }
        }
    }

    protected override int Count(Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        return institutionAdminRepository.Count(predicate).Result;
    }

    protected override IEnumerable<Ministry2AdminDto> Get(BaseAdminFilter filter, Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        var admins = institutionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: predicate,
                orderBy: MakeSortExpression(),
                asNoTracking: true);

        return mapper.Map<List<Ministry2AdminDto>>(admins);
    }

    protected override string GetCommunicationString(RequestCommand command) =>
        command switch
        {
            RequestCommand.Create => CommunicationConstants.CreateMinistryAdmin,
            RequestCommand.Update => CommunicationConstants.UpdateMinistryAdmin,
            RequestCommand.Delete => CommunicationConstants.DeleteMinistryAdmin,
            RequestCommand.Block => CommunicationConstants.BlockMinistryAdmin,
            RequestCommand.Reinvite => CommunicationConstants.ReinviteMinistryAdmin,
            _ => throw new ArgumentException("Invalid enum value for request command", nameof(command)),
        };

    protected override Expression<Func<InstitutionAdminBase, bool>> PredicateBuild(BaseAdminFilter filter)
    {
        var pred = base.PredicateBuild(filter);

        Expression<Func<InstitutionAdmin, bool>> predicate = expressionConverter.Convert(pred);

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<InstitutionAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if ((filter as Ministry2AdminFilter).InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == (filter as Ministry2AdminFilter).InstitutionId);
        }

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return expressionConverter2.Convert(predicate);
    }

    protected override async Task<bool> IsUserHasRightsToCreateAdmin(BaseAdminDto adminDto) => true;

    protected override async Task<bool> IsUserHasRightsToUpdateAdmin(string ministryAdminId)
    {
        if (currentUserService.UserId != ministryAdminId)
        {
            if (currentUserService.IsTechAdmin())
            {
                var ministryAdmin = await GetByIdAsync(ministryAdminId);

                if (ministryAdmin.AccountStatus == AccountStatus.Accepted)
                {
                    logger.LogDebug("Forbidden to update the accepted ministry admin.");

                    return false;
                }
            }
            else
            {
                logger.LogDebug("Forbidden to update the another ministry admin if you don't have the techadmin role.");

                return false;
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToDeleteAdmin(string adminId) => true;

    protected override async Task<bool> IsUserHasRightsToBlockAdmin(string adminId) => true;

    private Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection> MakeSortExpression() =>
        new Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection>
            {
                {
                    x => x.User.LastName,
                    SortDirection.Ascending
                },
            };
}