using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using Nest;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.StatusPermissions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Application entity.
/// </summary>
public class ApplicationService : IApplicationService, INotificationReciever
{
    private readonly IApplicationRepository applicationRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly ILogger<ApplicationService> logger;
    private readonly IMapper mapper;
    private readonly INotificationService notificationService;
    private readonly IProviderAdminService providerAdminService;
    private readonly IChangesLogService changesLogService;
    private readonly ApplicationsConstraintsConfig applicationsConstraintsConfig;
    private readonly IWorkshopServicesCombiner combinedWorkshopService;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;

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
    /// <param name="providerAdminService">Service for getting provider admins and deputies.</param>
    /// <param name="changesLogService">ChangesLogService.</param>
    /// <param name="combinedWorkshopService">WorkshopServicesCombiner.</param>
    /// <param name="currentUserService">Service for managing current user rights.</param>
    /// <param name="ministryAdminService">Service for managing ministry admin rigths.</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    /// <param name="codeficatorService">Codeficator service.</param>
    public ApplicationService(
        IApplicationRepository repository,
        ILogger<ApplicationService> logger,
        IWorkshopRepository workshopRepository,
        IMapper mapper,
        IOptions<ApplicationsConstraintsConfig> applicationsConstraintsConfig,
        INotificationService notificationService,
        IProviderAdminService providerAdminService,
        IChangesLogService changesLogService,
        IWorkshopServicesCombiner combinedWorkshopService,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService)
    {
        applicationRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.providerAdminService =
            providerAdminService ?? throw new ArgumentNullException(nameof(providerAdminService));
        this.changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
        this.applicationsConstraintsConfig = (applicationsConstraintsConfig ??
                                              throw new ArgumentNullException(nameof(applicationsConstraintsConfig))).Value;
        this.combinedWorkshopService =
            combinedWorkshopService ?? throw new ArgumentNullException(nameof(combinedWorkshopService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
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

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
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

        var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
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
                new ProviderAdminWorkshopRights(providerId, id));
        }

        filter ??= new ApplicationFilter();

        var predicate = PredicateBuild(filter, a => a.WorkshopId == id);

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);
        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
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
        var workshops = workshopRepository.Get(where: workshopFilter).Select(w => w.Id);

        var predicate = PredicateBuild(filter, a => workshops.Contains(a.WorkshopId));

        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);
        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
            orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

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
    public async Task<SearchResult<ApplicationDto>> GetAllByProviderAdmin(
        string userId,
        ApplicationFilter filter,
        Guid providerId = default,
        bool isDeputy = false)
    {
        logger.LogInformation(
            "Getting Applications by ProviderAdmin userId started. Looking ProviderAdmin userId = {UserId}", userId);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new ProviderAdminRights(userId));
        }

        filter ??= new ApplicationFilter();

        if (providerId == Guid.Empty)
        {
            FillProviderAdminInfo(userId, out providerId, out isDeputy);
        }

        List<Guid> workshopIds = new List<Guid>();

        if (!isDeputy)
        {
            workshopIds =
                (await providerAdminService.GetRelatedWorkshopIdsForProviderAdmins(userId).ConfigureAwait(false))
                .ToList();
        }

        Expression<Func<Workshop, bool>> workshopFilter =
            w => isDeputy ? w.ProviderId == providerId : workshopIds.Contains(w.Id);
        var workshops = workshopRepository.Get(where: workshopFilter).Select(w => w.Id);

        var predicate = PredicateBuild(filter, a => workshops.Contains(a.WorkshopId));
        var sortPredicate = SortExpressionBuild(filter);

        var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);

        var applications = await applicationRepository.Get(
            skip: filter.From,
            take: filter.Size,
            where: predicate,
            includeProperties: "Workshop,Child,Parent",
            orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

        logger.LogInformation(
            "There are {Count} applications in the Db with AdminProvider Id = {UserId}",
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

        Expression<Func<Application, bool>> filter = a => a.Id == id;

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
            new ProviderAdminWorkshopRights(application.Workshop.ProviderId, application.Workshop.Id));

        return mapper.Map<ApplicationDto>(application);
    }

    public Task<Result<ApplicationDto>> Update(ApplicationUpdate applicationDto, Guid providerId)
    {
        logger.LogInformation("Updating Application with Id = {Id} started", applicationDto?.Id);

        ArgumentNullException.ThrowIfNull(applicationDto, nameof(applicationDto));

        return ExecuteUpdateAsync(applicationDto, providerId);
    }

    public async Task<IEnumerable<string>> GetNotificationsRecipientIds(
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
            recipientIds.AddRange(await providerAdminService.GetProviderAdminsIds(application.Workshop.Id)
                .ConfigureAwait(false));
            recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(application.Workshop.Provider.Id)
                .ConfigureAwait(false));
        }
        else if (action == NotificationAction.Update)
        {
            if (additionalData != null
                && additionalData.ContainsKey("Status")
                && Enum.TryParse(additionalData["Status"], out ApplicationStatus applicationStatus))
            {
                if (applicationStatus == ApplicationStatus.Approved
                    || applicationStatus == ApplicationStatus.Rejected)
                {
                    recipientIds.Add(application.Parent.UserId);
                }
                else if (applicationStatus == ApplicationStatus.Left)
                {
                    recipientIds.Add(application.Workshop.Provider.UserId);
                    recipientIds.AddRange(await providerAdminService.GetProviderAdminsIds(application.Workshop.Id)
                        .ConfigureAwait(false));
                    recipientIds.AddRange(await providerAdminService
                        .GetProviderDeputiesIds(application.Workshop.Provider.Id).ConfigureAwait(false));
                }
            }
        }

        return recipientIds.Distinct();
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

    private static void UpdateStatus(ApplicationUpdate applicationDto, Application application, ICurrentUserService currentUserService)
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
        var userSubroleName = currentUserService.UserSubRole.ToLower();

        if (!applicationStatusPermissions.CanChangeStatus(userRole, userSubroleName, application.Status, applicationDto.Status))
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
                .Or(a => a.Child.FirstName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                         || a.Child.MiddleName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase)
                         || a.Child.LastName.StartsWith(filter.SearchString, StringComparison.InvariantCultureIgnoreCase));
            predicate = predicate.And(tempPredicate);
        }

        predicate = predicate.And(a => a.IsBlocked == filter.ShowBlocked);

        return predicate;
    }

    private static Dictionary<Expression<Func<Application, object>>, SortDirection> SortExpressionBuild(
        ApplicationFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Application, object>>, SortDirection>();

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

    private void FillProviderAdminInfo(string userId, out Guid providerId, out bool isDeputy)
    {
        var providerAdmin = providerAdminService.GetById(userId).GetAwaiter().GetResult();

        if (providerAdmin == null)
        {
            logger.LogError("ProviderAdmin with userId = {UserId} not exists", userId);

            throw new ArgumentException($"There is no providerAdmin with userId = {userId}");
        }

        providerId = providerAdmin.ProviderId;
        isDeputy = providerAdmin.IsDeputy;
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
            _ = await combinedWorkshopService.UpdateStatus(new Models.Workshop.WorkshopStatusDto
                { WorkshopId = workshopId, Status = WorkshopStatus.Closed });
        }
        else if (!isIncreaseTakenSeats && countTakenSeats == workshop.AvailableSeats - 1 &&
                 workshop.Status != WorkshopStatus.Open)
        {
            _ = await combinedWorkshopService.UpdateStatus(new Models.Workshop.WorkshopStatusDto
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
                { "Status", newApplication.Status.ToString() },
            };

            string groupedData = newApplication.Status.ToString();

            await notificationService.Create(
                NotificationType.Application,
                NotificationAction.Create,
                newApplication.Id,
                this,
                additionalData,
                groupedData).ConfigureAwait(false);
        }

        return new ModelWithAdditionalData<ApplicationDto, int>
        {
            Model = mapper.Map<ApplicationDto>(newApplication),
            AdditionalData = 0,
        };
    }

    private async Task<Result<ApplicationDto>> ExecuteUpdateAsync(ApplicationUpdate applicationDto, Guid providerId)
    {
        await currentUserService.UserHasRights(
            new ParentRights(applicationDto.ParentId),
            new ProviderRights(providerId),
            new ProviderAdminWorkshopRights(providerId, applicationDto.WorkshopId));

        var currentApplication = CheckApplicationExists(applicationDto.Id);

        if (currentApplication is null)
        {
            return Result<ApplicationDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Application does not exist.",
            });
        }

        var previewAppStatus = currentApplication.Status;

        try
        {
            if (currentApplication.Status == applicationDto.Status)
            {
                return Result<ApplicationDto>.Success(mapper.Map<ApplicationDto>(currentApplication));
            }

            if (applicationDto.Status == ApplicationStatus.Approved)
            {
                var providerOwnership = await workshopRepository.GetByFilter(predicate: w => w.Id == currentApplication.WorkshopId && w.Provider.Ownership != OwnershipType.State, includeProperties: "Provider");

                if (providerOwnership.Any())
                {
                    int amountOfApproved = await GetAmountOfApprovedApplications(currentApplication.WorkshopId);
                    uint availableSeats = await workshopRepository.GetAvailableSeats(currentApplication.WorkshopId);

                    if (amountOfApproved >= availableSeats)
                    {
                        return Result<ApplicationDto>.Failed(new OperationError
                        {
                            Code = "400",
                            Description = "The Workshop is full.",
                        });
                    }
                }
            }

            UpdateStatus(applicationDto, currentApplication, currentUserService);

            var updatedApplication = await applicationRepository.Update(
                    currentApplication,
                    x => changesLogService.AddEntityChangesToDbContext(x, currentUserService.UserId))
                .ConfigureAwait(false);

            logger.LogInformation("Application with Id = {Id} updated successfully", updatedApplication.Id);

            var additionalData = new Dictionary<string, string>()
            {
                { "Status", updatedApplication.Status.ToString() },
            };

            var groupedData = updatedApplication.Status.ToString();

            await notificationService.Create(
                NotificationType.Application,
                NotificationAction.Update,
                updatedApplication.Id,
                this,
                additionalData,
                groupedData).ConfigureAwait(false);

            await ControlWorkshopStatus(previewAppStatus, updatedApplication.Status, currentApplication.WorkshopId);

            return Result<ApplicationDto>.Success(mapper.Map<ApplicationDto>(updatedApplication));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Updating failed");
            throw;
        }
    }

    private async Task<int> GetAmountOfApprovedApplications(Guid workshopId)
    {
        return await applicationRepository.Count(x => x.WorkshopId == workshopId && x.Status == ApplicationStatus.Approved);
    }
}