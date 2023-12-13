using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;

namespace OutOfSchool.WebApi.Services.Admins;

public class Area2AdminService : BaseAdminService
{
    private const string IncludePropertiers = "Institution,User,CATOTTG.Parent.Parent";

    private readonly ILogger<Area2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IAreaAdminRepository areaAdminRepository;
    private readonly Ministry2AdminService ministryAdminService;
    private readonly Region2AdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;

    private readonly ExpressionConverter<InstitutionAdminBase, AreaAdmin> expressionConverter;
    private readonly ExpressionConverter<AreaAdmin, InstitutionAdminBase> expressionConverter2;

    public Area2AdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<Area2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IAreaAdminRepository areaAdminRepository,
        Ministry2AdminService ministryAdminService,
        Region2AdminService regionAdminService,
        ICodeficatorService codeficatorService)
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
        ArgumentNullException.ThrowIfNull(areaAdminRepository);
        ArgumentNullException.ThrowIfNull(ministryAdminService);
        ArgumentNullException.ThrowIfNull(regionAdminService);
        ArgumentNullException.ThrowIfNull(codeficatorService);

        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.areaAdminRepository = areaAdminRepository;
        this.ministryAdminService = ministryAdminService;
        this.regionAdminService = regionAdminService;
        this.codeficatorService = codeficatorService;

        expressionConverter = new ExpressionConverter<InstitutionAdminBase, AreaAdmin>();
        expressionConverter2 = new ExpressionConverter<AreaAdmin, InstitutionAdminBase>();
    }

    protected override async Task<BaseAdminDto> GetById(string id) =>
        mapper.Map<Area2AdminDto>(await areaAdminRepository.GetByIdAsync(id));

    protected override async Task<BaseAdminDto> GetByUserId(string userId) =>
        mapper.Map<Area2AdminDto>((await areaAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Area2AdminFilter CreateEmptyFilter() => new Area2AdminFilter();

    protected override async Task<bool> IsUserHasRightsToGetAdminsByFilter(BaseAdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId) as Ministry2AdminDto;

            if ((filter as Area2AdminFilter).InstitutionId != ministryAdmin.InstitutionId
                && (filter as Area2AdminFilter).InstitutionId != Guid.Empty)
            {
                return false;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync(currentUserService.UserId) as Region2AdminDto;
            var childrenCATOTTGIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

            if (((filter as Area2AdminFilter).InstitutionId != regionAdmin.InstitutionId
                && (filter as Area2AdminFilter).InstitutionId != Guid.Empty)
               || !(childrenCATOTTGIds.Contains((filter as Area2AdminFilter).CATOTTGId)
                    && (filter as Area2AdminFilter).CATOTTGId > 0))
            {
                return false;
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await GetByUserId(currentUserService.UserId) as Area2AdminDto;

            if (((filter as Area2AdminFilter).InstitutionId != areaAdmin.InstitutionId
                && (filter as Area2AdminFilter).InstitutionId != Guid.Empty)
               || ((filter as Area2AdminFilter).CATOTTGId != areaAdmin.CATOTTGId
                   && (filter as Area2AdminFilter).CATOTTGId > 0))
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

            if ((filter as Area2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Area2AdminFilter).InstitutionId = ministryAdmin.InstitutionId;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync(currentUserService.UserId) as Region2AdminDto;

            if ((filter as Area2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Area2AdminFilter).InstitutionId = regionAdmin.InstitutionId;
            }

            if ((filter as Area2AdminFilter).CATOTTGId == 0)
            {
                (filter as Area2AdminFilter).CATOTTGId = regionAdmin.CATOTTGId;
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await GetByUserId(currentUserService.UserId) as Area2AdminDto;

            if ((filter as Area2AdminFilter).InstitutionId == Guid.Empty)
            {
                (filter as Area2AdminFilter).InstitutionId = areaAdmin.InstitutionId;
            }

            if ((filter as Area2AdminFilter).CATOTTGId == 0)
            {
                (filter as Area2AdminFilter).CATOTTGId = areaAdmin.CATOTTGId;
            }
        }
    }

    protected override int Count(Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        return areaAdminRepository.Count(predicate).Result;
    }

    private Dictionary<Expression<Func<AreaAdmin, object>>, SortDirection> MakeSortExpression() =>
    new Dictionary<Expression<Func<AreaAdmin, object>>, SortDirection>
        {
            {
                x => x.User.LastName,
                SortDirection.Ascending
            },
        };

    protected override IEnumerable<Area2AdminDto> Get(BaseAdminFilter filter, Expression<Func<InstitutionAdminBase, bool>> filterPredicate)
    {
        var predicate = expressionConverter.Convert(filterPredicate);

        var admins = areaAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: predicate,
                orderBy: MakeSortExpression(),
                asNoTracking: true);

        return mapper.Map<List<Area2AdminDto>>(admins);
    }

    protected override string GetCommunicationString(RequestCommand command) =>
        command switch
        {
            RequestCommand.Create => CommunicationConstants.CreateAreaAdmin,
            RequestCommand.Update => CommunicationConstants.UpdateAreaAdmin,
            RequestCommand.Delete => CommunicationConstants.DeleteAreaAdmin,
            RequestCommand.Block => CommunicationConstants.BlockAreaAdmin,
            RequestCommand.Reinvite => CommunicationConstants.ReinviteAreaAdmin,
            _ => throw new ArgumentException("Invalid enum value for request command", nameof(command)),
        };

    protected override Expression<Func<InstitutionAdminBase, bool>> PredicateBuild(BaseAdminFilter filter)
    {
        var pred = base.PredicateBuild(filter);

        Expression<Func<AreaAdmin, bool>> predicate = expressionConverter.Convert(pred);

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<AreaAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.CATOTTG.Name.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if ((filter as Area2AdminFilter).InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == (filter as Area2AdminFilter).InstitutionId);
        }

        var childrenCATOTTGIds = codeficatorService.GetAllChildrenIdsByParentIdAsync((filter as Area2AdminFilter).CATOTTGId).Result;

        if (childrenCATOTTGIds.Any())
        {
            predicate = predicate.And(a => childrenCATOTTGIds.Contains(a.CATOTTGId));
        }
        else if ((filter as Area2AdminFilter).CATOTTGId > 0)
        {
            predicate = predicate.And(c => c.CATOTTG.Id == (filter as Area2AdminFilter).CATOTTGId);
        }

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return expressionConverter2.Convert(predicate);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAreaAdminSubordinatedToMinistryAdminAsync(string ministryAdminUserId, string areaAdminId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId) as Ministry2AdminDto;
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId);

        return ministryAdmin.InstitutionId == areaAdmin.InstitutionId;
    }

    public async Task<bool> IsAreaAdminSubordinatedToRegionAdminAsync(string regionAdminUserId, string areaAdminId)
    {
        _ = regionAdminUserId ?? throw new ArgumentNullException(nameof(regionAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var regionAdmin = await regionAdminService.GetByIdAsync(regionAdminUserId) as Region2AdminDto;
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId);

        var subSettlementsIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

        return regionAdmin.InstitutionId == areaAdmin.InstitutionId && subSettlementsIds.Contains(areaAdmin.CATOTTGId);
    }

    protected override async Task<bool> IsUserHasRightsToCreateAdmin(BaseAdminDto adminDto)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync((adminDto as Area2AdminDto).Id) as Ministry2AdminDto;

            if (ministryAdmin.InstitutionId != (adminDto as Area2AdminDto).InstitutionId)
            {
                logger.LogDebug("Forbidden to create area admin. Area admin isn't subordinated to ministry admin.");

                return false;
            }
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync((adminDto as Area2AdminDto).Id) as Region2AdminDto;

            var subSettlementsIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

            if (!(regionAdmin.InstitutionId == (adminDto as Area2AdminDto).InstitutionId
                && subSettlementsIds.Contains((adminDto as Area2AdminDto).CATOTTGId)))
            {
                logger.LogDebug("Forbidden to create area admin. Area admin isn't subordinated to region admin.");

                return false;
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToUpdateAdmin(string adminId)
    {
        if (currentUserService.UserId != adminId)
        {
            if (!(currentUserService.IsTechAdmin() || currentUserService.IsMinistryAdmin() || currentUserService.IsRegionAdmin()))
            {
                logger.LogDebug("Forbidden to update another area admin if you don't have techadmin, ministryadmin or regionadmin role.");

                return false;
            }

            var areaAdmin = await GetByIdAsync(adminId);

            if (areaAdmin.AccountStatus == AccountStatus.Accepted)
            {
                logger.LogDebug("Forbidden to update the accepted area admin.");

                return false;
            }

            if (currentUserService.IsMinistryAdmin())
            {
                if (!await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
                {
                    logger.LogDebug("Forbidden to update area admin. Area admin isn't subordinated to ministry admin.");

                    return false;
                }
            }

            if (currentUserService.IsRegionAdmin())
            {
                if (!await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId))
                {
                    logger.LogDebug("Forbidden to update area admin. Area admin isn't subordinated to region admin.");

                    return false;
                }
            }
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToDeleteAdmin(string adminId)
    {
        if (!(currentUserService.IsMinistryAdmin()
            && await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId)))
        {
            logger.LogDebug("Forbidden to delete area admin. Area admin isn't subordinated to ministry admin.");

            return false;
        }
        else if (!(currentUserService.IsRegionAdmin()
                 && await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId)))
        {
            logger.LogDebug("Forbidden to delete area admin. Area admin isn't subordinated to region admin.");

            return false;
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToBlockAdmin(string adminId)
    {
        if (!(currentUserService.IsMinistryAdmin()
            && await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId)))
        {
            logger.LogDebug("Forbidden to block area admin. Area admin isn't subordinated to ministry admin.");

            return false;
        }
        else if (!(currentUserService.IsRegionAdmin()
                 && await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId)))
        {
            logger.LogDebug("Forbidden to block area admin. Area admin isn't subordinated to region admin.");

            return false;
        }

        return true;
    }
}
