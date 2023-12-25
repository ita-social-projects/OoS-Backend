using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Admins;

public class Region2AdminService : BaseAdminService<RegionAdmin, Region2AdminDto, Region2AdminFilter>
{
    private const string IncludePropertiers = "Institution,User,CATOTTG";

    private readonly ILogger<Region2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly Ministry2AdminService ministryAdminService;

    public Region2AdminService(
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ILogger<Region2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IRegionAdminRepository regionAdminRepository,
        Ministry2AdminService ministryAdminService)
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
        ArgumentNullException.ThrowIfNull(regionAdminRepository);
        ArgumentNullException.ThrowIfNull(ministryAdminService);

        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.regionAdminRepository = regionAdminRepository;
        this.ministryAdminService = ministryAdminService;
    }

    protected override async Task<Region2AdminDto> GetById(string id) =>
        mapper.Map<Region2AdminDto>(await regionAdminRepository.GetByIdAsync(id));

    protected override async Task<Region2AdminDto> GetByUserId(string userId) =>
        mapper.Map<Region2AdminDto>((await regionAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Region2AdminFilter CreateEmptyFilter() => new();

    protected override async Task<bool> IsUserHasRightsToGetAdminsByFilter(Region2AdminFilter filter)
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
            var regionAdmin = await GetById(currentUserService.UserId);

            if ((filter.InstitutionId != regionAdmin.InstitutionId && filter.InstitutionId != Guid.Empty)
               || (filter.CATOTTGId != regionAdmin.CATOTTGId && filter.CATOTTGId > 0))
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task UpdateTheFilterWithTheAdminRestrictions(Region2AdminFilter filter)
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
            var regionAdmin = await GetById(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = regionAdmin.InstitutionId;
            }

            if (filter.CATOTTGId == 0)
            {
                filter.CATOTTGId = regionAdmin.CATOTTGId;
            }
        }
    }

    protected override int Count(Expression<Func<RegionAdmin, bool>> filterPredicate) =>
        regionAdminRepository.Count(filterPredicate).Result;

    protected override IEnumerable<Region2AdminDto> Get(Region2AdminFilter filter, Expression<Func<RegionAdmin, bool>> filterPredicate)
    {
        var admins = regionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: filterPredicate,
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

    protected override Expression<Func<RegionAdmin, bool>> PredicateBuild(Region2AdminFilter filter)
    {
        var predicate = PredicateBuilder.True<RegionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<RegionAdmin>();

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
            predicate = predicate.And(c => c.CATOTTG.Id == filter.CATOTTGId);
        }

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return predicate;
    }

    protected override async Task<bool> IsUserHasRightsToCreateAdmin(Region2AdminDto adminDto)
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId);

            if (ministryAdmin.InstitutionId != adminDto.InstitutionId)
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

            if (currentUserService.IsMinistryAdmin()
                && !await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
            {
                logger.LogDebug("Forbidden to update region admin. Region admin isn't subordinated to ministry admin.");

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
            && !await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to delete region admin. Region admin isn't subordinated to ministry admin.");

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
            && !await IsRegionAdminSubordinatedToMinistryAdminAsync(currentUserService.UserId, adminId))
        {
            logger.LogDebug("Forbidden to block region admin. Region admin isn't subordinated to ministry admin.");

            return false;
        }

        return true;
    }

    private Dictionary<Expression<Func<RegionAdmin, object>>, SortDirection> MakeSortExpression() =>
        new()
        {
            {
                x => x.User.LastName,
                SortDirection.Ascending
            },
        };

    private async Task<bool> IsRegionAdminSubordinatedToMinistryAdminAsync(string ministryAdminUserId, string regionAdminId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId);
        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId);

        return ministryAdmin.InstitutionId == regionAdmin.InstitutionId;
    }
}
