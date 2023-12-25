using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Admins;

public class Area2AdminService : BaseAdminService<AreaAdmin, Area2AdminDto, Area2AdminFilter>
{
    private const string IncludePropertiers = "Institution,User,CATOTTG.Parent.Parent";

    private readonly ILogger<Area2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IAreaAdminRepository areaAdminRepository;
    private readonly Ministry2AdminService ministryAdminService;
    private readonly Region2AdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;

    public Area2AdminService(
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ILogger<Area2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IAreaAdminRepository areaAdminRepository,
        Ministry2AdminService ministryAdminService,
        Region2AdminService regionAdminService,
        ICodeficatorService codeficatorService)
        : base(
            authorizationServerConfig,
            communicationService,
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
    }

    protected override async Task<Area2AdminDto> GetById(string id) =>
        mapper.Map<Area2AdminDto>(await areaAdminRepository.GetByIdAsync(id));

    protected override async Task<Area2AdminDto> GetByUserId(string userId) =>
        mapper.Map<Area2AdminDto>((await areaAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Area2AdminFilter CreateEmptyFilter() => new();

    protected override async Task<bool> IsUserHasRightsToGetAdminsByFilter(Area2AdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId);

            if (filter.InstitutionId != ministryAdmin.InstitutionId && filter.InstitutionId != Guid.Empty)
            {
                return false;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync(currentUserService.UserId);
            var childrenCATOTTGIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

            if ((filter.InstitutionId != regionAdmin.InstitutionId && filter.InstitutionId != Guid.Empty)
                || (!childrenCATOTTGIds.Contains(filter.CATOTTGId) && filter.CATOTTGId > 0))
            {
                return false;
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await GetByUserId(currentUserService.UserId);

            if ((filter.InstitutionId != areaAdmin.InstitutionId && filter.InstitutionId != Guid.Empty)
               || (filter.CATOTTGId != areaAdmin.CATOTTGId && filter.CATOTTGId > 0))
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task UpdateTheFilterWithTheAdminRestrictions(Area2AdminFilter filter)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = ministryAdmin.InstitutionId;
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = regionAdmin.InstitutionId;
            }

            if (filter.CATOTTGId == 0)
            {
                filter.CATOTTGId = regionAdmin.CATOTTGId;
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await GetByUserId(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = areaAdmin.InstitutionId;
            }

            if (filter.CATOTTGId == 0)
            {
                filter.CATOTTGId = areaAdmin.CATOTTGId;
            }
        }
    }

    protected override int Count(Expression<Func<AreaAdmin, bool>> filterPredicate) =>
        areaAdminRepository.Count(filterPredicate).Result;

    protected override IEnumerable<Area2AdminDto> Get(Area2AdminFilter filter, Expression<Func<AreaAdmin, bool>> filterPredicate)
    {
        var admins = areaAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: filterPredicate,
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

    protected override Expression<Func<AreaAdmin, bool>> PredicateBuild(Area2AdminFilter filter)
    {
        var predicate = PredicateBuilder.True<AreaAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<AreaAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.CATOTTG.Name.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == filter.InstitutionId);
        }

        if (filter.CATOTTGId > 0)
        {
            var childrenCATOTTGIds = codeficatorService.GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId).Result;

            if (childrenCATOTTGIds.Any())
            {
                predicate = predicate.And(a => childrenCATOTTGIds.Contains(a.CATOTTGId));
            }
            else
            {
                predicate = predicate.And(a => a.CATOTTG.Id == filter.CATOTTGId);
            }
        }

        predicate = predicate.And<AreaAdmin>(x => !x.Institution.IsDeleted);

        return predicate;
    }

    protected override async Task<bool> IsUserHasRightsToCreateAdmin(Area2AdminDto adminDto)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId);

            if (ministryAdmin.InstitutionId != adminDto.InstitutionId)
            {
                logger.LogDebug("Forbidden to create area admin. Area admin isn't subordinated to ministry admin.");

                return false;
            }
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByIdAsync(currentUserService.UserId);

            var subSettlementsIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

            if (!(regionAdmin.InstitutionId == adminDto.InstitutionId && subSettlementsIds.Contains(adminDto.CATOTTGId)))
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

            if (currentUserService.IsMinistryAdmin()
                && !await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
            {
                logger.LogDebug("Forbidden to update area admin. Area admin isn't subordinated to ministry admin.");

                return false;
            }

            if (currentUserService.IsRegionAdmin()
                && !await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId))
            {
                logger.LogDebug("Forbidden to update area admin. Area admin isn't subordinated to region admin.");

                return false;
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

        if (currentUserService.IsMinistryAdmin()
            && !await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to delete area admin. Area admin isn't subordinated to ministry admin.");

            return false;
        }

        if (currentUserService.IsRegionAdmin()
            && !await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to delete area admin. Area admin isn't subordinated to region admin.");

            return false;
        }

        return true;
    }

    protected override async Task<bool> IsUserHasRightsToBlockAdmin(string adminId)
    {
        if (currentUserService.IsTechAdmin())
        {
            return true;
        }

        if (currentUserService.IsMinistryAdmin()
            && !await IsAreaAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to block area admin. Area admin isn't subordinated to ministry admin.");

            return false;
        }

        if (currentUserService.IsRegionAdmin()
            && !await IsAreaAdminSubordinatedToRegionAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to block area admin. Area admin isn't subordinated to region admin.");

            return false;
        }

        return true;
    }

    private Dictionary<Expression<Func<AreaAdmin, object>>, SortDirection> MakeSortExpression() =>
        new()
        {
            {
                x => x.User.LastName,
                SortDirection.Ascending
            },
        };

    private async Task<bool> IsAreaAdminSubordinatedToMinistryAdminAsync(string ministryAdminUserId, string areaAdminId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId);
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId);

        return ministryAdmin.InstitutionId == areaAdmin.InstitutionId;
    }

    private async Task<bool> IsAreaAdminSubordinatedToRegionAdminAsync(string regionAdminUserId, string areaAdminId)
    {
        _ = regionAdminUserId ?? throw new ArgumentNullException(nameof(regionAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var regionAdmin = await regionAdminService.GetByIdAsync(regionAdminUserId);
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId);

        var subSettlementsIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId);

        return regionAdmin.InstitutionId == areaAdmin.InstitutionId && subSettlementsIds.Contains(areaAdmin.CATOTTGId);
    }
}
