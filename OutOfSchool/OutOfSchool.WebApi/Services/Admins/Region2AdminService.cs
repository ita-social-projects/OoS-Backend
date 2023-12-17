using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;

namespace OutOfSchool.WebApi.Services.Admins;

public class Region2AdminService : BaseAdminService
{
    private const string IncludePropertiers = "Institution,User,CATOTTG";

    private readonly ILogger<Region2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly Ministry2AdminService ministryAdminService;

    private readonly ExpressionConverter<InstitutionAdminBase, RegionAdmin> expressionConverter;
    private readonly ExpressionConverter<RegionAdmin, InstitutionAdminBase> expressionConverter2;

    public Region2AdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<Region2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IRegionAdminRepository regionAdminRepository,
        Ministry2AdminService ministry2AdminService)
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
        ArgumentNullException.ThrowIfNull(regionAdminRepository);
        ArgumentNullException.ThrowIfNull(ministry2AdminService);

        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.regionAdminRepository = regionAdminRepository;
        this.ministryAdminService = ministry2AdminService;

        expressionConverter = new ExpressionConverter<InstitutionAdminBase, RegionAdmin>();
        expressionConverter2 = new ExpressionConverter<RegionAdmin, InstitutionAdminBase>();
    }

    protected override async Task<BaseAdminDto> GetById(string id) =>
        mapper.Map<Region2AdminDto>(await regionAdminRepository.GetByIdAsync(id));

    protected override async Task<BaseAdminDto> GetByUserId(string userId) =>
        mapper.Map<Region2AdminDto>((await regionAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Region2AdminFilter CreateEmptyFilter() => new Region2AdminFilter();

    protected override async Task<bool> IsUserHasRightsToGetAdminsByFilter(BaseAdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId) as Ministry2AdminDto;

            if ((filter as Region2AdminFilter).InstitutionId != ministryAdmin.InstitutionId
                && (filter as Region2AdminFilter).InstitutionId != Guid.Empty)
            {
                return false;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await GetById(currentUserService.UserId) as Region2AdminDto;

            if (((filter as Region2AdminFilter).InstitutionId != regionAdmin.InstitutionId
                && (filter as Region2AdminFilter).InstitutionId != Guid.Empty)
               || ((filter as Region2AdminFilter).CATOTTGId != regionAdmin.CATOTTGId
                   && (filter as Region2AdminFilter).CATOTTGId > 0))
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
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId) as Ministry2AdminDto;

            if ((filter as Region2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Region2AdminFilter).InstitutionId = ministryAdmin.InstitutionId;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await GetById(currentUserService.UserId) as Region2AdminDto;

            if ((filter as Region2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Region2AdminFilter).InstitutionId = regionAdmin.InstitutionId;
            }

            if ((filter as Region2AdminFilter).CATOTTGId == 0)
            {
                (filter as Region2AdminFilter).CATOTTGId = regionAdmin.CATOTTGId;
            }
        }
    }

    protected override int Count(Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        return regionAdminRepository.Count(predicate).Result;
    }

    private Dictionary<Expression<Func<RegionAdmin, object>>, SortDirection> MakeSortExpression() =>
    new Dictionary<Expression<Func<RegionAdmin, object>>, SortDirection>
        {
            {
                x => x.User.LastName,
                SortDirection.Ascending
            },
        };

    protected override IEnumerable<Region2AdminDto> Get(BaseAdminFilter filter, Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        var admins = regionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: predicate,
                orderBy: MakeSortExpression(),
                asNoTracking: true);

        return mapper.Map<List<Region2AdminDto>>(admins);
    }

    protected override string GetCommunicationString(RequestCommand command) =>
    command switch
    {
        RequestCommand.Create => CommunicationConstants.CreateRegionAdmin,
        RequestCommand.Update => CommunicationConstants.UpdateRegionAdmin,
        RequestCommand.Delete => CommunicationConstants.DeleteRegionAdmin,
        RequestCommand.Block => CommunicationConstants.BlockRegionAdmin,
        RequestCommand.Reinvite => CommunicationConstants.ReinviteRegionAdmin,
        _ => throw new ArgumentException("Invalid enum value for request command", nameof(command)),
    };

    protected override Expression<Func<InstitutionAdminBase, bool>> PredicateBuild(BaseAdminFilter filter)
    {
        var pred = base.PredicateBuild(filter);

        Expression<Func<RegionAdmin, bool>> predicate = expressionConverter.Convert(pred);

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<RegionAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.CATOTTG.Name.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if ((filter as Region2AdminFilter).InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == (filter as Region2AdminFilter).InstitutionId);
        }

        if ((filter as Region2AdminFilter).CATOTTGId > 0)
        {
            predicate = predicate.And(c => c.CATOTTG.Id == (filter as Region2AdminFilter).CATOTTGId);
        }

        return expressionConverter2.Convert(predicate);
    }

    public async Task<bool> IsRegionAdminSubordinatedToMinistryAdminAsync(string ministryAdminUserId, string regionAdminId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId) as Ministry2AdminDto;
        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId);

        return ministryAdmin.InstitutionId == regionAdmin.InstitutionId;
    }

    protected override async Task<bool> IsUserHasRightsToCreateAdmin(BaseAdminDto adminDto)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId) as Ministry2AdminDto;

            if (ministryAdmin.InstitutionId != (adminDto as Region2AdminDto).InstitutionId)
            {
                logger.LogDebug("Forbidden to create region admin. Region admin isn't subordinated to ministry admin.");

                return false;
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToUpdateAdmin(string adminId)
    {
        if (currentUserService.UserId != adminId)
        {
            if (!(currentUserService.IsTechAdmin() || currentUserService.IsMinistryAdmin()))
            {
                logger.LogDebug("Forbidden to update another region admin if you don't have techadmin or ministryadmin role.");

                return false;
            }

            var regionAdmin = await GetByIdAsync(adminId);

            if (regionAdmin.AccountStatus == AccountStatus.Accepted)
            {
                logger.LogDebug("Forbidden to update the accepted region admin.");

                return false;
            }

            if (currentUserService.IsMinistryAdmin())
            {
                if (!await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
                {
                    logger.LogDebug("Forbidden to update region admin. Region admin isn't subordinated to ministry admin.");

                    return false;
                }
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToDeleteAdmin(string adminId)
    {
        if (currentUserService.IsTechAdmin())
        {
            return true;
        }

        if (currentUserService.IsMinistryAdmin())
        {
            if (!await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
            {
                logger.LogDebug("Forbidden to delete region admin. Region admin isn't subordinated to ministry admin.");

                return false;
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToBlockAdmin(string adminId)
    {
        if (currentUserService.IsTechAdmin())
        {
            return true;
        }

        if (currentUserService.IsMinistryAdmin())
        {
            if (!await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
            {
                logger.LogDebug("Forbidden to block region admin. Region admin isn't subordinated to ministry admin.");

                return false;
            }
        }

        return true;
    }
}
