using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Application entity.
    /// </summary>
    public class ApplicationService : IApplicationService, INotificationReciever
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IEntityRepository<Child> childRepository;
        private readonly ILogger<ApplicationService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly IProviderAdminService providerAdminService;
        private readonly IChangesLogService changesLogService;
        private readonly ApplicationsConstraintsConfig applicationsConstraintsConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationService"/> class.
        /// </summary>
        /// <param name="repository">Application repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="workshopRepository">Workshop repository.</param>
        /// <param name="childRepository">Child repository.</param>
        /// <param name="mapper">Automapper DI service.</param>
        /// <param name="applicationsConstraintsConfig">Options for application's constraints.</param>
        /// <param name="notificationService">Notification service.</param>
        /// <param name="providerAdminService">Service for getting provider admins and deputies.</param>
        /// <param name="changesLogService">ChangesLogService.</param>
        public ApplicationService(
            IApplicationRepository repository,
            ILogger<ApplicationService> logger,
            IStringLocalizer<SharedResource> localizer,
            IWorkshopRepository workshopRepository,
            IEntityRepository<Child> childRepository,
            IMapper mapper,
            IOptions<ApplicationsConstraintsConfig> applicationsConstraintsConfig,
            INotificationService notificationService,
            IProviderAdminService providerAdminService,
            IChangesLogService changesLogService)
        {
            applicationRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            this.providerAdminService = providerAdminService ?? throw new ArgumentNullException(nameof(providerAdminService));
            this.changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
            this.applicationsConstraintsConfig = (applicationsConstraintsConfig ?? throw new ArgumentNullException(nameof(applicationsConstraintsConfig))).Value;
        }

        /// <inheritdoc/>
        public async Task<ModelWithAdditionalData<ApplicationDto, int>> Create(ApplicationDto applicationDto)
        {
            logger.LogInformation("Application creating started.");

            ModelNullValidation(applicationDto);

            var allowedNewApplicationForChild = await AllowedNewApplicationByChildStatus(applicationDto.WorkshopId, applicationDto.ChildId).ConfigureAwait(false);

            if (!allowedNewApplicationForChild)
            {
                logger.LogInformation("Unable to create a new application for a child because there's already appropriate status were found in this workshop.");
                throw new ArgumentException("Unable to create a new application for a child because there's already appropriate status were found in this workshop.");
            }

            (bool IsCorrect, int SecondsRetryAfter) resultOfCheck = await CheckApplicationsLimit(applicationDto).ConfigureAwait(false);

            if (!resultOfCheck.IsCorrect)
            {
                return new ModelWithAdditionalData<ApplicationDto, int>
                {
                    Description = $"Limit of applications per {applicationsConstraintsConfig.ApplicationsLimitDays} days is exceeded.",
                    AdditionalData = resultOfCheck.SecondsRetryAfter,
                };
            }

            var isChildParent = await CheckChildParent(applicationDto.ParentId, applicationDto.ChildId).ConfigureAwait(false);

            if (!isChildParent)
            {
                logger.LogInformation("Operation failed. Unable to create application for another parent`s child.");
                throw new ArgumentException(localizer["Unable to create application for another parent`s child."]);
            }

            var application = mapper.Map<Application>(applicationDto);

            var newApplication = await applicationRepository.Create(application).ConfigureAwait(false);

            logger.LogInformation($"Application with Id = {newApplication?.Id} created successfully.");

            if (newApplication != null)
            {
                var additionalData = new Dictionary<string, string>()
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

        /// <inheritdoc/>
        [Obsolete("This method doesn't check application restrictions")]
        public async Task<IEnumerable<ApplicationDto>> Create(IEnumerable<ApplicationDto> applicationDtos)
        {
            logger.LogInformation("Multiple applications creating started.");

            MultipleModelCreationValidation(applicationDtos);

            var applications = mapper.Map<List<Application>>(applicationDtos);

            var newApplications = await applicationRepository.Create(applications).ConfigureAwait(false);

            logger.LogInformation("Applications created successfully.");

            return mapper.Map<List<ApplicationDto>>(newApplications);
        }

        /// <inheritdoc/>
        public async Task Delete(Guid id)
        {
            logger.LogInformation($"Deleting Application with Id = {id} started.");

            CheckApplicationExists(id);

            var application = new Application { Id = id };

            try
            {
                await applicationRepository.Delete(application).ConfigureAwait(false);

                logger.LogInformation($"Application with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError($"Deleting failed. Exception: {ex.Message}.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAll()
        {
            logger.LogInformation("Getting all Applications started.");

            var applications = await applicationRepository.GetAllWithDetails("Workshop,Child,Parent").ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? "Application table is empty."
                : $"All {applications.Count()} records were successfully received from the Application table");

            return mapper.Map<List<ApplicationDto>>(applications);
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ApplicationDto>> GetAllByParent(Guid id, ApplicationFilter filter)
        {
            logger.LogInformation($"Getting Applications by Parent Id started. Looking Parent Id = {id}.");
            FilterNullValidation(filter);

            var predicate = PredicateBuild(filter, a => a.ParentId == id);

            var sortPredicate = SortExpressionBuild(filter);

            var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);

            var applications = await applicationRepository.Get(
                skip: filter.From,
                take: filter.Size,
                where: predicate,
                includeProperties: "Workshop,Child,Parent",
                orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Parent Id = {id}."
                : $"Successfully got Applications with Parent Id = {id}.");

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
            logger.LogInformation($"Getting Applications by Child Id started. Looking Child Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.ChildId == id;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Child Id = {id}."
                : $"Successfully got Applications with Child Id = {id}.");

            return mapper.Map<List<ApplicationDto>>(applications);
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ApplicationDto>> GetAllByWorkshop(Guid id, ApplicationFilter filter)
        {
            logger.LogInformation($"Getting Applications by Workshop Id started. Looking Workshop Id = {id}.");

            FilterNullValidation(filter);

            var predicate = PredicateBuild(filter, a => a.WorkshopId == id);

            var sortPredicate = SortExpressionBuild(filter);

            var totalAmount = await applicationRepository.Count(where: predicate).ConfigureAwait(false);
            var applications = await applicationRepository.Get(
                skip: filter.From,
                take: filter.Size,
                where: predicate,
                includeProperties: "Workshop,Child,Parent",
                orderBy: sortPredicate).ToListAsync().ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Workshop Id = {id}."
                : $"Successfully got Applications with Workshop Id = {id}.");

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
            logger.LogInformation($"Getting Applications by Provider Id started. Looking Provider Id = {id}.");

            FilterNullValidation(filter);

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

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Provider Id = {id}."
                : $"Successfully got Applications with Provider Id = {id}.");

            var searchResult = new SearchResult<ApplicationDto>()
            {
                TotalAmount = totalAmount,
                Entities = mapper.Map<List<ApplicationDto>>(applications),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ApplicationDto>> GetAllByProviderAdmin(string userId, ApplicationFilter filter, Guid providerId = default, bool isDeputy = false)
        {
            logger.LogInformation($"Getting Applications by ProviderAdmin userId started. Looking ProviderAdmin userId = {userId}.");

            FilterNullValidation(filter);

            if (providerId == Guid.Empty)
            {
                FillProviderAdminInfo(userId, out providerId, out isDeputy);
            }

            List<Guid> workshopIds = new List<Guid>();

            if (!isDeputy)
            {
                workshopIds = (await providerAdminService.GetRelatedWorkshopIdsForProviderAdmins(userId).ConfigureAwait(false)).ToList();
            }

            Expression<Func<Workshop, bool>> workshopFilter = w => isDeputy ? w.ProviderId == providerId : workshopIds.Contains(w.Id);
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

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with AdminProvider Id = {userId}."
                : $"Successfully got Applications with AdminProvider Id = {userId}.");

            var searchResult = new SearchResult<ApplicationDto>()
            {
                TotalAmount = totalAmount,
                Entities = mapper.Map<List<ApplicationDto>>(applications),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByStatus(int status)
        {
            logger.LogInformation($"Getting Applications by Status started. Looking Status = {status}.");

            Expression<Func<Application, bool>> filter = a => (int)a.Status == status;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Status = {status}."
                : $"Successfully got Applications with Status = {status}.");

            return mapper.Map<List<ApplicationDto>>(applications);
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> GetById(Guid id)
        {
            logger.LogInformation($"Getting Application by Id started. Looking Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.Id == id;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);
            var application = applications.FirstOrDefault();

            if (application is null)
            {
                logger.LogInformation($"There is no application in the Db with Id = {id}.");
                return null;
            }

            logger.LogInformation($"Successfully got an Application with Id = {id}.");

            return mapper.Map<ApplicationDto>(application);
        }

        public async Task<ApplicationDto> Update(ApplicationDto applicationDto, string userId)
        {
            logger.LogInformation($"Updating Application with Id = {applicationDto?.Id} started.");

            ModelNullValidation(applicationDto);

            CheckApplicationExists(applicationDto.Id);

            try
            {
                var updatedApplication = await applicationRepository.Update(
                    mapper.Map<Application>(applicationDto),
                    x => changesLogService.AddEntityChangesToDbContext(x, userId))
                    .ConfigureAwait(false);

                logger.LogInformation($"Application with Id = {applicationDto?.Id} updated succesfully.");

                var currentApplication = await applicationRepository.GetById(applicationDto.Id).ConfigureAwait(false);

                if (currentApplication.Status != updatedApplication.Status)
                {
                    var additionalData = new Dictionary<string, string>()
                    {
                        { "Status", updatedApplication.Status.ToString() },
                    };

                    string groupedData = updatedApplication.Status.ToString();

                    await notificationService.Create(
                        NotificationType.Application,
                        NotificationAction.Update,
                        updatedApplication.Id,
                        this,
                        additionalData,
                        groupedData).ConfigureAwait(false);
                }

                return mapper.Map<ApplicationDto>(updatedApplication);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError($"Updating failed. Exception = {ex.Message}.");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
        {
            var recipientIds = new List<string>();

            var applications = await applicationRepository.GetByFilter(a => a.Id == objectId, "Workshop.Provider.User").ConfigureAwait(false);
            var application = applications.FirstOrDefault();

            if (application is null)
            {
                return recipientIds;
            }

            if (action == NotificationAction.Create)
            {
                recipientIds.Add(application.Workshop.Provider.UserId);
                recipientIds.AddRange(await providerAdminService.GetProviderAdminsIds(application.Workshop.Id).ConfigureAwait(false));
                recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(application.Workshop.Provider.Id).ConfigureAwait(false));
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
                        recipientIds.AddRange(await providerAdminService.GetProviderAdminsIds(application.Workshop.Id).ConfigureAwait(false));
                        recipientIds.AddRange(await providerAdminService.GetProviderDeputiesIds(application.Workshop.Provider.Id).ConfigureAwait(false));
                    }
                }
            }

            return recipientIds.Distinct();
        }

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

        private void ModelNullValidation(ApplicationDto applicationDto)
        {
            if (applicationDto is null)
            {
                logger.LogInformation("Operation failed. ApplicationDto is null");
                throw new ArgumentException(localizer["Application dto is null."], nameof(applicationDto));
            }
        }

        private void FilterNullValidation(ApplicationFilter filter)
        {
            if (filter is null)
            {
                logger.LogInformation("Operation failed. Application filter is null.");
                throw new ArgumentException(localizer["Application filter is null."], nameof(filter));
            }
        }

        private void MultipleModelCreationValidation(IEnumerable<ApplicationDto> applicationDtos)
        {
            if (!applicationDtos.Any())
            {
                logger.LogInformation("Operation failed. There is no application to create.");
                throw new ArgumentException(localizer["There is no application to create."]);
            }

            foreach (var application in applicationDtos)
            {
                ModelNullValidation(application);
            }
        }

        private void CheckApplicationExists(Guid id)
        {
            var applications = applicationRepository.Get(where: a => a.Id == id);

            if (!applications.Any())
            {
                logger.LogInformation($"Operation failed. Application with Id = {id} doesn't exist in the system.");
                throw new ArgumentException(localizer[$"Application with Id = {id} doesn't exist in the system."]);
            }
        }

        private async Task<bool> CheckChildParent(Guid parentId, Guid childId)
        {
            Expression<Func<Child, bool>> filter = c => c.ParentId == parentId;

            var children = childRepository.Get(where: filter).Select(c => c.Id);

            return await children.ContainsAsync(childId).ConfigureAwait(false);
        }

        private async Task<(bool IsCorrect, int SecondsRetryAfter)> CheckApplicationsLimit(ApplicationDto applicationDto)
        {
            var endDate = applicationDto.CreationTime;

            var startDate = endDate.AddDays(-applicationsConstraintsConfig.ApplicationsLimitDays);

            Expression<Func<Application, bool>> filter = a => a.ChildId == applicationDto.ChildId
                                                                && a.WorkshopId == applicationDto.WorkshopId
                                                                && a.ParentId == applicationDto.ParentId
                                                                && (a.CreationTime >= startDate && a.CreationTime <= endDate);

            var applications = (await applicationRepository.GetByFilter(filter).ConfigureAwait(false)).ToArray();

            if (applications.Length >= applicationsConstraintsConfig.ApplicationsLimit)
            {
                logger.LogInformation($"Limit of applications per {applicationsConstraintsConfig.ApplicationsLimitDays} days is exceeded.");

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

        private Expression<Func<Application, bool>> PredicateBuild(
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

            predicate = predicate.And(a => a.IsBlocked == filter.ShowBlocked);

            return predicate;
        }

        private Dictionary<Expression<Func<Application, object>>, SortDirection> SortExpressionBuild(ApplicationFilter filter)
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

        private void FillProviderAdminInfo(string userId, out Guid providerId, out bool isDeputy)
        {
            var providerAdmin = providerAdminService.GetById(userId).GetAwaiter().GetResult();

            if (providerAdmin == null)
            {
                logger.LogInformation($"ProviderAdmin with userId = {userId} not exists.");

                throw new ArgumentException(localizer[$"There is no providerAdmin with userId = {userId}"]);
            }

            providerId = providerAdmin.ProviderId;
            isDeputy = providerAdmin.IsDeputy;
        }
    }
}
