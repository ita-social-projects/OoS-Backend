using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.Admins;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Admins;

public class Ministry2AdminService : BaseAdminService<InstitutionAdmin, Ministry2AdminDto, Ministry2AdminFilter>
{
    private const string IncludePropertiers = "Institution,User";

    private readonly ILogger<Ministry2AdminService> logger;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IInstitutionAdminRepository institutionAdminRepository;

    public Ministry2AdminService(
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ILogger<Ministry2AdminService> logger,
        IMapper mapper,
        IUserService userService,
        ICurrentUserService currentUserService,
        IInstitutionAdminRepository institutionAdminRepository)
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
        ArgumentNullException.ThrowIfNull(institutionAdminRepository);

        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.institutionAdminRepository = institutionAdminRepository;
    }

    protected override async Task<Ministry2AdminDto> GetById(string id) =>
        mapper.Map<Ministry2AdminDto>(await institutionAdminRepository.GetByIdAsync(id));

    protected override async Task<Ministry2AdminDto> GetByUserId(string userId) =>
        mapper.Map<Ministry2AdminDto>((await institutionAdminRepository.GetByFilter(p => p.UserId == userId)).FirstOrDefault());

    protected override Ministry2AdminFilter CreateEmptyFilter() => new();

    protected override async Task<bool> UserHasRightsToGetAdminsByFilter(Ministry2AdminFilter filter) => true;

    protected override async Task<bool> UserHasRightsToCreateAdmin(Ministry2AdminDto adminDto) => true;

    protected override async Task<bool> UserHasRightsToUpdateAdmin(string adminId)
    {
        if (currentUserService.UserId != adminId)
        {
            if (!currentUserService.IsTechAdmin())
            {
                logger.LogDebug("Forbidden to update the another ministry admin if you don't have the techadmin role.");

                return false;
            }

            var ministryAdmin = await GetByIdAsync(adminId);

            if (ministryAdmin.AccountStatus == AccountStatus.Accepted)
            {
                logger.LogDebug("Forbidden to update the accepted ministry admin.");

                return false;
            }
        }

        return true;
    }

    protected override async Task<bool> UserHasRightsToDeleteAdmin(string adminId) => true;

    protected override async Task<bool> UserHasRightsToBlockAdmin(string adminId) => true;

    protected override async Task UpdateTheFilterWithTheAdminRestrictions(Ministry2AdminFilter filter)
    {
    }

    protected override int Count(Expression<Func<InstitutionAdmin, bool>> filterPredicate) =>
        institutionAdminRepository.Count(filterPredicate).Result;

    protected override IEnumerable<Ministry2AdminDto> Get(Ministry2AdminFilter filter, Expression<Func<InstitutionAdmin, bool>> filterPredicate)
    {
        var admins = institutionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: IncludePropertiers,
                whereExpression: filterPredicate,
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

    protected override Expression<Func<InstitutionAdmin, bool>> PredicateBuild(Ministry2AdminFilter filter)
    {
        var predicate = PredicateBuilder.True<InstitutionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<InstitutionAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == filter.InstitutionId);
        }

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return predicate;
    }

    private Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection> MakeSortExpression() =>
        new()
        {
            {
                x => x.User.LastName,
                SortDirection.Ascending
            },
        };
}