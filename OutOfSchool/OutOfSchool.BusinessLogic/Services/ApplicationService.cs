using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Common.StatusPermissions;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Application entity.
/// </summary>
public class ApplicationService : IApplicationService, ISensitiveApplicationService
{
    public const string UaMaleEnding = "ий";
    public const string UaFemaleEnding = "а";
    public const string UaUnspecifiedGenderEnding = "ий/a";
    private const string StatusTitle = "Status";

    private readonly IApplicationRepository applicationRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly ILogger<ApplicationService> logger;
    private readonly IMapper mapper;
    private readonly INotificationService notificationService;
    private readonly IEmployeeService employeeService;
    private readonly IChangesLogService changesLogService;
    private readonly ApplicationsConstraintsConfig applicationsConstraintsConfig;
    private readonly IWorkshopServicesCombiner combinedWorkshopService;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly IAreaAdminService areaAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IEmailSenderService emailSender;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IOptions<HostsConfig> hostsConfig;

    private readonly string errorNullWorkshopMessage = "Operation failed. Workshop in Application dto is null";
    private readonly string errorBlockedWorkshopMessage = "Unable to create a new application for a workshop because workshop is blocked";
    private readonly string errorClosedWorkshopMessage = "Unable to create a new application for a workshop because workshop status is closed";
    private readonly string errorNoAllowedNewApplicationMessage = "Unable to create a new application for a child because there's already appropriate status were found in this workshop";

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="repository">Application repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="workshopRepository">Workshop repository.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="applicationsConstraintsConfig">Options for application's constraints.</param>
    /// <param name="notificationService">Notification service.</param>
    /// <param name="employeeService">Service for getting provider admins and deputies.</param>
    /// <param name="changesLogService">ChangesLogService.</param>
    /// <param name="combinedWorkshopService">WorkshopServicesCombiner.</param>
    /// <param name="currentUserService">Service for managing current user rights.</param>
    /// <param name="ministryAdminService">Service for managing ministry admin rigths.</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    /// <param name="areaAdminService">Service for managing area admin rigths.</param>
    /// <param name="codeficatorService">Codeficator service.</param>
    /// <param name="renderer">Razor view to string renderer.</param>
    /// <param name="emailSender">Service for sending Email messages.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="hostsConfig">Hosts config.</param>
    public ApplicationService(
        IApplicationRepository repository,
        ILogger<ApplicationService> logger,
        IWorkshopRepository workshopRepository,
        IMapper mapper,
        IOptions<ApplicationsConstraintsConfig> applicationsConstraintsConfig,
        INotificationService notificationService,
        IEmployeeService employeeService,
        IChangesLogService changesLogService,
        IWorkshopServicesCombiner combinedWorkshopService,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        IAreaAdminService areaAdminService,
        ICodeficatorService codeficatorService,
        IRazorViewToStringRenderer renderer,
        IEmailSenderService emailSender,
        IStringLocalizer<SharedResource> localizer,
        IOptions<HostsConfig> hostsConfig)
    {
        applicationRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.employeeService =
            employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        this.changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
        this.applicationsConstraintsConfig = (applicationsConstraintsConfig ??
                                              throw new ArgumentNullException(nameof(applicationsConstraintsConfig))).Value;
        this.combinedWorkshopService =
            combinedWorkshopService ?? throw new ArgumentNullException(nameof(combinedWorkshopService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.areaAdminService = areaAdminService ?? throw new ArgumentNullException(nameof(areaAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(emailSender));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.hostsConfig = hostsConfig ?? throw new ArgumentNullException(nameof(hostsConfig));
    }

    /// <inheritdoc/>
    public Task<ModelWithAdditionalData<ApplicationDto, int>> Create(ApplicationCreate applicationDto)
    {
        logger.LogInformation("Application creating started");

        ArgumentNullException.ThrowIfNull(applicationDto, nameof(applicationDto));

        return ExecuteCreateAsync(applicationDto);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ApplicationDto>> GetAll(ApplicationFilter filter)
    {
        logger.LogInformation("Getting all Applications started");

        if (!currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation");
        }

        filter ??= new ApplicationFilter();

        var predicate = PredicateBuild(filter);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByIdAsync(currentUserService.UserId);
            predicate = predicate
                .And(p => p.Workshop.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate
                .And(p => p.Workshop.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<Application>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Workshop.Provider.LegalAddress.CATOTTGId == item);
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate
                .And(p => p.Workshop.InstitutionHierarchy.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<Application>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Workshop.Provider.LegalAddress.CATOTTGId == item);
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            includeProperties: "Workshop,Child,Parent",
            whereExpression: predicate,
            orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

        logger.LogInformation("There are {Count} applications in the Db", applications.Count);

        var searchResult = new SearchResult<ApplicationDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ApplicationDto>>(applications),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ApplicationDto>> GetAllByParent(Guid id, ApplicationFilter filter)
    {
        logger.LogInformation("Getting Applications by Parent Id started. Looking Parent Id = {Id}", id);
        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new ParentRights(id));
        }

        filter ??= new ApplicationFilter();

        var predicate = PredicateBuild(filter, a => a.ParentId == id);

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            includeProperties: "Workshop,Child,Parent",
            whereExpression: predicate,
            orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

        logger.LogInformation("There are {Count} applications in the Db with Parent Id = {Id}", applications.Count, id);

        var searchResult = new SearchResult<ApplicationDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ApplicationDto>>(applications),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<int> GetCountByParentId(Guid id)
    {
        logger.LogInformation("Getting Applications count by Parent Id started. Looking Parent Id = {Id}", id);
        if (!currentUserService.IsInRole(Role.Provider) && !currentUserService.IsEmployeeOrProvider())
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation");
        }

        var totalAmount = await applicationRepository.Count(a => a.ParentId == id).ConfigureAwait(false);

        logger.LogInformation("There are {Count} applications in the Db with Parent Id = {Id}", totalAmount, id);

        return totalAmount;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ApplicationDto>> GetAllByChild(Guid id)
    {
        logger.LogInformation("Getting Applications by Child Id started. Looking Child Id = {Id}", id);

        Expression<Func<Application, bool>> filter = a => false;

        if (currentUserService.IsInRole(Role.Parent))
        {
            filter = a => a.ChildId == id && a.Parent.UserId == currentUserService.UserId;
        }
        else if (currentUserService.IsInRole(Role.Provider))
        {
            filter = a => a.ChildId == id && a.Workshop.Provider.UserId == currentUserService.UserId;
        }
        else if (currentUserService.IsAdmin())
        {
            filter = a => a.ChildId == id;
        }

        var applications = (await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent")).ToList();

        logger.LogInformation("There are {Count} applications in the Db with Parent Id = {Id}", applications.Count, id);

        return mapper.Map<List<ApplicationDto>>(applications);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ApplicationDto>> GetAllByWorkshop(Guid id, Guid providerId, ApplicationFilter filter)
    {
        logger.LogInformation("Getting Applications by Workshop Id started. Looking Workshop Id = {Id}", id);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(
                new ProviderRights(providerId),
                new EmployeeWorkshopRights(providerId, id));
        }

        filter ??= new ApplicationFilter();

        var predicate = PredicateBuild(filter, a => a.WorkshopId == id);

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(whereExpression: predicate).ConfigureAwait(false);
        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            includeProperties: "Workshop,Child,Parent",
            whereExpression: predicate,
            orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

        logger.LogInformation(
            "There are {Count} applications in the Db with Workshop Id = {Id}",
            applications.Count,
            id);

        var searchResult = new SearchResult<ApplicationDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ApplicationDto>>(applications),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ApplicationDto>> GetAllByProvider(Guid id, ApplicationFilter filter)
    {
        logger.LogInformation("Getting Applications by Provider Id started. Looking Provider Id = {Id}", id);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new ProviderRights(id));
        }

        filter ??= new ApplicationFilter();

        Expression<Func<Workshop, bool>> workshopFilter = w => w.ProviderId == id;
        var workshops = workshopRepository.Get(whereExpression: workshopFilter).Select(w => w.Id);

        var predicate = PredicateBuild(filter, a => workshops.Contains(a.WorkshopId));

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(whereExpression: predicate).ConfigureAwait(false);
        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            whereExpression: predicate,
            orderBy: sortPredicate)
            .Include(a => a.Workshop).ThenInclude(w => w.Address).ThenInclude(wa => wa.CATOTTG)
                .ThenInclude(wac => wac.Parent).ThenInclude(wacp => wacp.Parent).ThenInclude(wacpp => wacpp.Parent).ThenInclude(wacppp => wacppp.Parent)
            .Include(a => a.Workshop).ThenInclude(w => w.InstitutionHierarchy).ThenInclude(wi => wi.Institution)
            .Include(a => a.Workshop).ThenInclude(w => w.InstitutionHierarchy).ThenInclude(wi => wi.Directions)
            .Include(a => a.Workshop).ThenInclude(w => w.Applications).ThenInclude(wa => wa.Child)
            .Include(a => a.Workshop).ThenInclude(w => w.Applications).ThenInclude(wa => wa.Parent)
            .Include(a => a.Workshop).ThenInclude(w => w.Provider).ThenInclude(p => p.User)
            .Include(a => a.Child).ThenInclude(c => c.SocialGroups)
            .Include(a => a.Parent).ThenInclude(p => p.User)
            .ToListAsync().ConfigureAwait(false);

        logger.LogInformation(
            "There are {Count} applications in the Db with Provider Id = {Id}",
            applications.Count,
            id);

        var searchResult = new SearchResult<ApplicationDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ApplicationDto>>(applications),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ApplicationDto>> GetAllByEmployee(
        string userId,
        ApplicationFilter filter,
        Guid providerId = default,
        bool isDeputy = false)
    {
        logger.LogInformation(
            "Getting Applications by Employee userId started. Looking employee userId = {UserId}", userId);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new EmployeeRights(userId));
        }

        filter ??= new ApplicationFilter();

        if (providerId == Guid.Empty)
        {
            this.FillEmployeeInfo(userId, out providerId);
        }

        List<Guid> workshopIds = new List<Guid>();

        workshopIds =
            (await employeeService.GetRelatedWorkshopIdsForEmployees(userId).ConfigureAwait(false))
            .ToList();

        Expression<Func<Workshop, bool>> workshopFilter =
            w => isDeputy ? w.ProviderId == providerId : workshopIds.Contains(w.Id);
        var workshops = workshopRepository.Get(whereExpression: workshopFilter).Select(w => w.Id);

        var predicate = PredicateBuild(filter, a => workshops.Contains(a.WorkshopId));
        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            includeProperties: "Workshop,Child,Parent",
            whereExpression: predicate, orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

        logger.LogInformation(
            "There are {Count} applications in the Db with employee Id = {UserId}",
            applications.Count,
            userId);

        var searchResult = new SearchResult<ApplicationDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ApplicationDto>>(applications),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<ApplicationDto> GetById(Guid id)
    {
        logger.LogInformation("Getting Application by Id started. Looking Id = {Id}", id);

        Expression<Func<Application, bool>> filter = a => a.Id == id && !a.Child.IsDeleted && !a.Parent.IsDeleted && !a.Workshop.IsDeleted;

        var applications =
            await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);
        var application = applications.FirstOrDefault();

        if (application is null)
        {
            logger.LogInformation("There is no application in the Db with Id = {Id}", id);
            return null;
        }

        logger.LogInformation("Successfully got an Application with Id = {Id}", id);

        await currentUserService.UserHasRights(
            new ParentRights(application.ParentId),
            new ProviderRights(application.Workshop.ProviderId),
            new EmployeeWorkshopRights(application.Workshop.ProviderId, application.Workshop.Id));

        return mapper.Map<ApplicationDto>(application);
    }

    public Task<Either<ErrorResponse, ApplicationDto>> Update(ApplicationUpdate applicationDto, Guid providerId)
    {
        logger.LogInformation("Updating Application with Id = {Id} started", applicationDto?.Id);

        ArgumentNullException.ThrowIfNull(applicationDto, nameof(applicationDto));
        return ExecuteUpdateAsync(applicationDto, providerId);
    }

    /// <inheritdoc/>
    public async Task<bool> AllowedNewApplicationByChildStatus(Guid workshopId, Guid childId)
    {
        var forbiddenStatuses = new[]
        {
            ApplicationStatus.Pending,
            ApplicationStatus.AcceptedForSelection,
            ApplicationStatus.Approved,
            ApplicationStatus.StudyingForYears,
        };

        Expression<Func<Application, bool>> filter = a => a.ChildId == childId
                                                          && a.WorkshopId == workshopId
                                                          && forbiddenStatuses.Contains(a.Status);

        return !await applicationRepository.Any(filter).ConfigureAwait(false);
    }

    public async Task<bool> AllowedToReview(Guid parentId, Guid workshopId)
    {
        var statuses = new[]
        {
            ApplicationStatus.Completed,
            ApplicationStatus.Approved,
            ApplicationStatus.StudyingForYears,
        };

        Expression<Func<Application, bool>> filter = a => a.ParentId == parentId
                                                          && a.WorkshopId == workshopId
                                                          && statuses.Contains(a.Status);

        return await applicationRepository.Any(filter).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> ChangeApprovedStatusesToStudying()
    {
        var result = await applicationRepository.UpdateAllApprovedApplications().ConfigureAwait(false);

        logger.LogInformation("Updated statuses to Studying of {count} applications.", result);

        return result;
    }

    private void UpdateStatus(ApplicationUpdate applicationDto, Application application)
    {
        if (application.Status == applicationDto.Status)
        {
            return;
        }

        var applicationStatusPermissions = new ApplicationStatusPermissions();

        if (application.Workshop.CompetitiveSelection)
        {
            applicationStatusPermissions.InitCompetitiveSelectionPermissions();
        }
        else
        {
            applicationStatusPermissions.InitDefaultPermissions();
        }

        var userRole = currentUserService.UserRole.ToLower();

        if (!applicationStatusPermissions.CanChangeStatus(userRole, application.Status, applicationDto.Status))
        {
            throw new ArgumentException("Forbidden to update status from " + application.Status + " to " + applicationDto.Status);
        }

        application.Status = applicationDto.Status;
        application.RejectionMessage = applicationDto.RejectionMessage;

        if (application.Status != ApplicationStatus.Rejected)
        {
            application.RejectionMessage = null;
        }

        if (application.Status == ApplicationStatus.Approved)
        {
            application.ApprovedTime = DateTimeOffset.UtcNow;
        }
    }

    private static Expression<Func<Application, bool>> PredicateBuild(
        ApplicationFilter filter,
        Expression<Func<Application, bool>> predicate = null)
    {
        if (predicate is null)
        {
            predicate = PredicateBuilder.True<Application>();
        }

        predicate = predicate.And(a => !a.Child.IsDeleted && !a.Parent.IsDeleted && !a.Workshop.IsDeleted);

        if (filter.Statuses != null)
        {
            predicate = predicate.And(a => filter.Statuses.Contains(a.Status));
        }

        if (filter.Workshops != null)
        {
            var tempPredicate = PredicateBuilder.False<Application>();

            foreach (var workshop in filter.Workshops)
            {
                tempPredicate = tempPredicate.Or(a => a.WorkshopId == workshop);
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.Children != null)
        {
            var tempPredicate = PredicateBuilder.False<Application>();

            foreach (var child in filter.Children)
            {
                tempPredicate = tempPredicate.Or(a => a.ChildId == child);
            }

            predicate = predicate.And(tempPredicate);
        }

        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<Application>();
            tempPredicate = tempPredicate
                .Or(a => a.Workshop.Title == filter.SearchString)
                .Or(a => a.Workshop.ProviderTitle.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase))
                .Or(a => a.Workshop.ProviderTitleEn.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase))
                .Or(a => a.Child.FirstName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                         || a.Child.MiddleName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                         || a.Child.LastName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase));
            predicate = predicate.And(tempPredicate);
        }

        if (filter.Show != ShowApplications.All)
        {
            predicate = predicate.And(a => a.IsBlockedByProvider == (filter.Show == ShowApplications.Blocked));
        }

        return predicate;
    }

    private static Dictionary<Expression<Func<Application, object>>, SortDirection> SortExpressionBuild(
        ApplicationFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Application, object>>, SortDirection>();

        if (filter.Show == ShowApplications.All)
        {
            sortExpression.Add(a => a.IsBlockedByProvider, SortDirection.Ascending);
        }

        if (filter.OrderByStatus)
        {
            sortExpression.Add(a => a.Status, SortDirection.Ascending);
        }

        if (filter.OrderByDateAscending)
        {
            sortExpression.Add(a => a.CreationTime, SortDirection.Ascending);
        }

        if (filter.OrderByAlphabetically)
        {
            sortExpression.Add(a => a.Parent.User.LastName, SortDirection.Ascending);
        }

        return sortExpression;
    }

    private async Task<bool> IsNewApplicationAllowed(Guid workshopId)
    {
        var workshop = await combinedWorkshopService.GetById(workshopId).ConfigureAwait(false);

        if (workshop is not null)
        {
            return workshop.Status == WorkshopStatus.Open;
        }

        logger.LogError(this.errorNullWorkshopMessage);
        throw new ArgumentException(@"Workshop in Application dto is null.", nameof(workshopId));
    }

    private async Task<bool> IsBlokedWorkshop(Guid workshopId)
    {
        var workshop = await combinedWorkshopService.GetById(workshopId).ConfigureAwait(false);

        if (workshop is not null)
        {
            return workshop.IsBlocked;
        }

        logger.LogError(this.errorNullWorkshopMessage);
        throw new ArgumentException(@"Workshop in Application dto is null.", nameof(workshopId));
    }

    private Application CheckApplicationExists(Guid id)
    {
        var application = applicationRepository.GetById(id).Result;

        if (application == null)
        {
            logger.LogInformation("Application with Id = {Id} doesn't exist in the system", id);
        }

        return application;
    }

    private async Task<(bool IsCorrect, int SecondsRetryAfter)> CheckApplicationsLimit(ApplicationCreate applicationDto)
    {
        var endDate = DateTimeOffset.UtcNow;

        var startDate = endDate.AddDays(-applicationsConstraintsConfig.ApplicationsLimitDays);

        Expression<Func<Application, bool>> filter = a => a.ChildId == applicationDto.ChildId
                                                          && a.WorkshopId == applicationDto.WorkshopId
                                                          && a.ParentId == applicationDto.ParentId
                                                          && (a.CreationTime >= startDate && a.CreationTime <= endDate);

        var applications = (await applicationRepository.GetByFilter(filter).ConfigureAwait(false)).ToArray();

        if (applications.Length >= applicationsConstraintsConfig.ApplicationsLimit)
        {
            logger.LogInformation(
                "Limit of applications per {Limit} days is exceeded",
                applicationsConstraintsConfig.ApplicationsLimitDays);

            DateTimeOffset dateStartingSendNewApplication = applications
                .OrderByDescending(a => a.CreationTime)
                .Take(applicationsConstraintsConfig.ApplicationsLimit)
                .Last()
                .CreationTime
                .AddDays(applicationsConstraintsConfig.ApplicationsLimitDays)
                .AddSeconds(1);

            return (IsCorrect: false,
                SecondsRetryAfter: (int)dateStartingSendNewApplication.Subtract(DateTimeOffset.UtcNow).TotalSeconds);
        }

        return (IsCorrect: true, SecondsRetryAfter: 0);
    }

    private void FillEmployeeInfo(string userId, out Guid providerId)
    {
        var employee = employeeService.GetById(userId).GetAwaiter().GetResult();

        if (employee == null)
        {
            logger.LogError("employee with userId = {UserId} not exists", userId);

            throw new ArgumentException($"There is no employee with userId = {userId}");
        }

        providerId = employee.ProviderId;
    }

    private async Task ControlWorkshopStatus(
        ApplicationStatus previewStatus,
        ApplicationStatus newStatus,
        Guid workshopId)
    {
        var takenSeatsStatuses = new[]
        {
            ApplicationStatus.Approved,
            ApplicationStatus.StudyingForYears,
        };

        var notTakenSeatsStatuses = new[]
        {
            ApplicationStatus.Pending,
            ApplicationStatus.AcceptedForSelection,
            ApplicationStatus.Rejected,
            ApplicationStatus.Completed,
            ApplicationStatus.Left,
        };

        if (takenSeatsStatuses.Contains(newStatus) && notTakenSeatsStatuses.Contains(previewStatus))
        {
            await ControlWorkshopStatus(workshopId, true);
        }
        else if (takenSeatsStatuses.Contains(previewStatus) && notTakenSeatsStatuses.Contains(newStatus))
        {
            await ControlWorkshopStatus(workshopId);
        }
    }

    private async Task ControlWorkshopStatus(Guid workshopId, bool isIncreaseTakenSeats = false)
    {
        var countTakenSeats = await GetTakenSeats(workshopId);

        var workshop = await combinedWorkshopService.GetById(workshopId).ConfigureAwait(false);

        if (isIncreaseTakenSeats && countTakenSeats == workshop.AvailableSeats &&
            workshop.Status != WorkshopStatus.Closed)
        {
            _ = await combinedWorkshopService.UpdateStatus(new WorkshopStatusDto
                { WorkshopId = workshopId, Status = WorkshopStatus.Closed });
        }
        else if (!isIncreaseTakenSeats && countTakenSeats == workshop.AvailableSeats - 1 &&
                 workshop.Status != WorkshopStatus.Open)
        {
            _ = await combinedWorkshopService.UpdateStatus(new WorkshopStatusDto
                { WorkshopId = workshopId, Status = WorkshopStatus.Open });
        }
    }

    private async ValueTask<int> GetTakenSeats(Guid workshopId)
    {
        return await applicationRepository
            .Count(x => x.WorkshopId == workshopId &&
                        (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears))
            .ConfigureAwait(false);
    }

    private async Task<ModelWithAdditionalData<ApplicationDto, int>> ExecuteCreateAsync(ApplicationCreate applicationDto)
    {
        await currentUserService.UserHasRights(new ParentRights(applicationDto.ParentId, applicationDto.ChildId));

        if (await IsBlokedWorkshop(applicationDto.WorkshopId))
        {
            logger.LogError(this.errorBlockedWorkshopMessage);
            throw new ArgumentException(this.errorBlockedWorkshopMessage);
        }

        var isNewApplicationAllowed = await IsNewApplicationAllowed(applicationDto.WorkshopId).ConfigureAwait(false);

        if (!isNewApplicationAllowed)
        {
            logger.LogError(this.errorClosedWorkshopMessage);
            throw new ArgumentException(this.errorClosedWorkshopMessage);
        }

        var allowedNewApplicationForChild =
            await AllowedNewApplicationByChildStatus(applicationDto.WorkshopId, applicationDto.ChildId)
                .ConfigureAwait(false);

        if (!allowedNewApplicationForChild)
        {
            logger.LogError(this.errorNoAllowedNewApplicationMessage);
            throw new ArgumentException(this.errorNoAllowedNewApplicationMessage);
        }

        (bool IsCorrect, int SecondsRetryAfter) resultOfCheck =
            await CheckApplicationsLimit(applicationDto).ConfigureAwait(false);

        if (!resultOfCheck.IsCorrect)
        {
            return new ModelWithAdditionalData<ApplicationDto, int>
            {
                Description =
                    $"Limit of applications per {applicationsConstraintsConfig.ApplicationsLimitDays} days is exceeded.",
                AdditionalData = resultOfCheck.SecondsRetryAfter,
            };
        }

        var application = mapper.Map<Application>(applicationDto);

        application.Id = Guid.Empty;

        application.CreationTime = DateTimeOffset.UtcNow;

        application.Status = ApplicationStatus.Pending;

        var newApplication = await applicationRepository.Create(application).ConfigureAwait(false);

        logger.LogInformation("Application with Id = {Id} created successfully", newApplication?.Id);

        if (newApplication != null)
        {
            var additionalData = new Dictionary<string, string>
            {
                { StatusTitle, newApplication.Status.ToString() },
            };

            string groupedData = newApplication.Status.ToString();
            var recipientsIds = await GetNotificationsRecipientIds(NotificationAction.Create, additionalData, newApplication.Id).ConfigureAwait(false);

            await notificationService.Create(
                NotificationType.Application,
                NotificationAction.Create,
                newApplication.Id,
                recipientsIds,
                additionalData,
                groupedData).ConfigureAwait(false);
        }

        return new ModelWithAdditionalData<ApplicationDto, int>
        {
            Model = mapper.Map<ApplicationDto>(newApplication),
            AdditionalData = 0,
        };
    }

    private async Task<Either<ErrorResponse, ApplicationDto>> ExecuteUpdateAsync(ApplicationUpdate applicationDto, Guid providerId)
    {
        await currentUserService.UserHasRights(
            new ParentRights(applicationDto.ParentId),
            new ProviderRights(providerId),
            new EmployeeWorkshopRights(providerId, applicationDto.WorkshopId));
        var currentApplication = this.CheckApplicationExists(applicationDto.Id);

        if (currentApplication is null)
        {
            return ErrorResponse.BadRequest(ApiErrorsTypes.Common.EntityIdDoesNotExist("Application", applicationDto.Id.ToString()).ToResponse());
        }

        var previewAppStatus = currentApplication.Status;

        try
        {
            if (currentApplication.Status == applicationDto.Status)
            {
                return mapper.Map<ApplicationDto>(currentApplication);
            }

            if (Application.ValidApplicationStatuses.Contains(applicationDto.Status))
            {
                var providerOwnership = await workshopRepository.GetByFilter(whereExpression: w => w.Id == currentApplication.WorkshopId && w.Provider.Ownership != OwnershipType.State, includeProperties: "Provider");

                if (providerOwnership.Any())
                {
                    int amountOfApproved = await GetAmountOfApprovedApplications(currentApplication.WorkshopId);
                    uint availableSeats = await workshopRepository.GetAvailableSeats(currentApplication.WorkshopId);

                    if (amountOfApproved >= availableSeats)
                    {
                        return ErrorResponse.BadRequest(ApiErrorsTypes.Application.AcceptRejectedWorkshopIsFull().ToResponse());
                    }
                }

                if (await GetApprovedWorkshopAndChild(currentApplication) >= 1)
                {
                    return ErrorResponse.BadRequest(ApiErrorsTypes.Application.AcceptRejectedAlreadyApproved().ToResponse());
                }
            }

            UpdateStatus(applicationDto, currentApplication);

            var updatedApplication = await applicationRepository.Update(
                    currentApplication,
                    x => changesLogService.AddEntityChangesToDbContext(x, currentUserService.UserId))
                .ConfigureAwait(false);

            logger.LogInformation("Application with Id = {Id} updated successfully", updatedApplication.Id);

            var additionalData = new Dictionary<string, string>()
            {
                { StatusTitle, updatedApplication.Status.ToString() },
            };

            var groupedData = updatedApplication.Status.ToString();
            var recipientsIds = await GetNotificationsRecipientIds(NotificationAction.Update, additionalData, updatedApplication.Id).ConfigureAwait(false);

            await notificationService.Create(
                NotificationType.Application,
                NotificationAction.Update,
                updatedApplication.Id,
                recipientsIds,
                additionalData,
                groupedData).ConfigureAwait(false);

            if (GetStatusesForParentsNotification().Contains(updatedApplication.Status))
            {
                await SendApplicationUpdateStatusEmail(updatedApplication);
            }

            await ControlWorkshopStatus(previewAppStatus, updatedApplication.Status, currentApplication.WorkshopId);

            return mapper.Map<ApplicationDto>(updatedApplication);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Updating failed");
            throw;
        }
    }

    private ApplicationStatus[] GetStatusesForParentsNotification()
    {
        return new ApplicationStatus[] {ApplicationStatus.Approved, ApplicationStatus.Rejected, ApplicationStatus.AcceptedForSelection };
    }

    private async Task<IEnumerable<string>> GetNotificationsRecipientIds(
        NotificationAction action,
        Dictionary<string, string> additionalData,
        Guid objectId)
    {
        var recipientIds = new List<string>();

        var applications = await applicationRepository.GetByFilter(a => a.Id == objectId, "Workshop.Provider.User")
            .ConfigureAwait(false);
        var application = applications.FirstOrDefault();

        if (application is null)
        {
            return recipientIds;
        }

        if (action == NotificationAction.Create)
        {
            recipientIds.Add(application.Workshop.Provider.UserId);
            recipientIds.AddRange(await employeeService.GetEmployeesIds(application.Workshop.Id)
                .ConfigureAwait(false));
            recipientIds.AddRange(await employeeService.GetEmployeesIds(application.Workshop.Provider.Id)
                .ConfigureAwait(false));
        }
        else if (action == NotificationAction.Update)
        {
            if (additionalData != null
                && additionalData.ContainsKey(StatusTitle)
                && Enum.TryParse(additionalData[StatusTitle], out ApplicationStatus applicationStatus))
            {
                if (GetStatusesForParentsNotification().Contains(applicationStatus))
                {
                    recipientIds.Add(application.Parent.UserId);
                }
                else if (applicationStatus == ApplicationStatus.Left)
                {
                    recipientIds.Add(application.Workshop.Provider.UserId);
                    recipientIds.AddRange(await employeeService.GetEmployeesIds(application.Workshop.Id)
                        .ConfigureAwait(false));
                    recipientIds.AddRange(await employeeService
                        .GetEmployeesIds(application.Workshop.Provider.Id).ConfigureAwait(false));
                }
            }
        }

        return recipientIds.Distinct();
    }

    private async Task<int> GetAmountOfApprovedApplications(Guid workshopId)
    {
        return await applicationRepository.Count(x => x.WorkshopId == workshopId &&
                                                      Application.ValidApplicationStatuses.Contains(x.Status) &&
                                                      !x.Child.IsDeleted &&
                                                      !x.Parent.IsDeleted);
    }

    private async Task<int> GetApprovedWorkshopAndChild(Application application)
    {
        return await applicationRepository.Count(a => application.ChildId == a.ChildId &&
                                                      application.WorkshopId == a.WorkshopId &&
                                                      Application.ValidApplicationStatuses.Contains(a.Status));
    }

    private async Task SendApplicationUpdateStatusEmail(Application application)
    {
        (string subject, string templateName) = application.Status switch
        {
            ApplicationStatus.Approved =>
                (localizer["Approved!"], RazorTemplates.ApplicationApprovedEmail),
            ApplicationStatus.Rejected =>
                (localizer["Rejected"], RazorTemplates.ApplicationRejectedEmail),
            ApplicationStatus.AcceptedForSelection =>
                (localizer["Accepted for selection!"], RazorTemplates.ApplicationAcceptedForSelectionEmail),
            _ => throw new ArgumentException("Unsupported application status."),
        };

        var applicationStatusViewModel = new ApplicationStatusViewModel
        {
            ChildFullName =
                $"{application.Child.LastName} " +
                $"{application.Child.FirstName} " +
                $"{application.Child.MiddleName}".TrimEnd(),
            UaEnding = application.Child.Gender switch
            {
                Gender.Male => UaMaleEnding,
                Gender.Female => UaFemaleEnding,
                _ => UaUnspecifiedGenderEnding,
            },
            WorkshopTitle = application.Workshop.Title,
            WorkshopUrl =
                $"{hostsConfig.Value.FrontendUrl}{hostsConfig.Value.PathToWorkshopDetailsOnFrontend}{application.WorkshopId}",
            RejectionMessage = application.RejectionMessage,
        };
        var content = await renderer
                .GetHtmlPlainStringAsync(templateName, applicationStatusViewModel);
        await emailSender.SendAsync(application.Parent.User.Email, subject, content);
    }
}
