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
using Newtonsoft.Json;
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
        public ApplicationService(
            IApplicationRepository repository,
            ILogger<ApplicationService> logger,
            IStringLocalizer<SharedResource> localizer,
            IWorkshopRepository workshopRepository,
            IEntityRepository<Child> childRepository,
            IMapper mapper,
            IOptions<ApplicationsConstraintsConfig> applicationsConstraintsConfig,
            INotificationService notificationService)
        {
            this.applicationRepository = repository;
            this.workshopRepository = workshopRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.childRepository = childRepository;
            this.mapper = mapper;
            this.notificationService = notificationService;

            try
            {
                this.applicationsConstraintsConfig = applicationsConstraintsConfig.Value;
            }
            catch (OptionsValidationException ex)
            {
                foreach (var failure in ex.Failures)
                {
                    logger.LogError(failure);
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ModelWithAdditionalData<ApplicationDto, int>> Create(ApplicationDto applicationDto)
        {
            logger.LogInformation("Application creating started.");

            ModelNullValidation(applicationDto);

            (bool IsCorrect, int SecondsRetryAfter) resultOfCheck = await CheckApplicationsLimit(applicationDto).ConfigureAwait(false);

            if (!resultOfCheck.IsCorrect)
            {
                var modelData = new ModelWithAdditionalData<ApplicationDto, int>()
                {
                    Description = $"Limit of applications per {applicationsConstraintsConfig.ApplicationsLimitDays} days is exceeded.",
                    AdditionalData = resultOfCheck.SecondsRetryAfter,
                };

                return modelData;
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
                await notificationService.Create(
                    NotificationType.Application,
                    NotificationAction.Create,
                    newApplication.Id,
                    this).ConfigureAwait(false);
            }

            return new ModelWithAdditionalData<ApplicationDto, int>()
            {
                Model = mapper.Map<ApplicationDto>(newApplication),
                AdditionalData = 0,
            };
        }

        /// <inheritdoc/>
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

                await notificationService.Create(
                    NotificationType.Application,
                    NotificationAction.Delete,
                    application.Id,
                    this).ConfigureAwait(false);
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
        public async Task<IEnumerable<ApplicationDto>> GetAllByParent(Guid id)
        {
            logger.LogInformation($"Getting Applications by Parent Id started. Looking Parent Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.ParentId == id;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.LogInformation(!applications.Any()
                ? $"There is no applications in the Db with Parent Id = {id}."
                : $"Successfully got Applications with Parent Id = {id}.");

            return mapper.Map<List<ApplicationDto>>(applications);
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
        public async Task<IEnumerable<ApplicationDto>> GetAllByWorkshop(Guid id, ApplicationFilter filter)
        {
            logger.LogInformation($"Getting Applications by Workshop Id started. Looking Workshop Id = {id}.");

            FilterNullValidation(filter);

            Expression<Func<Application, bool>> applicationFilter = a => a.WorkshopId == id;
            var applications = applicationRepository.Get<int>(where: applicationFilter, includeProperties: "Workshop,Child,Parent");

            var filteredApplications = await GetFiltered(applications, filter).ToListAsync().ConfigureAwait(false);

            logger.LogInformation(!filteredApplications.Any()
                ? $"There is no applications in the Db with Workshop Id = {id}."
                : $"Successfully got Applications with Workshop Id = {id}.");

            return mapper.Map<List<ApplicationDto>>(filteredApplications);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByProvider(Guid id, ApplicationFilter filter)
        {
            logger.LogInformation($"Getting Applications by Provider Id started. Looking Provider Id = {id}.");

            FilterNullValidation(filter);

            Expression<Func<Workshop, bool>> workshopFilter = w => w.ProviderId == id;
            var workshops = workshopRepository.Get<int>(where: workshopFilter).Select(w => w.Id);

            Expression<Func<Application, bool>> applicationFilter = a => workshops.Contains(a.WorkshopId);
            var applications = applicationRepository.Get<int>(where: applicationFilter, includeProperties: "Workshop,Child,Parent");

            var filteredApplications = await GetFiltered(applications, filter).ToListAsync().ConfigureAwait(false);

            logger.LogInformation(!filteredApplications.Any()
                ? $"There is no applications in the Db with Provider Id = {id}."
                : $"Successfully got Applications with Provider Id = {id}.");

            return mapper.Map<List<ApplicationDto>>(filteredApplications);
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

        public async Task<ApplicationDto> Update(ApplicationDto applicationDto)
        {
            logger.LogInformation($"Updating Application with Id = {applicationDto?.Id} started.");

            ModelNullValidation(applicationDto);

            CheckApplicationExists(applicationDto.Id);

            try
            {
                var updatedApplication = await applicationRepository.Update(mapper.Map<Application>(applicationDto))
                    .ConfigureAwait(false);

                logger.LogInformation($"Application with Id = {applicationDto?.Id} updated succesfully.");

                var additionalData = new Dictionary<string, string>()
                {
                    { "Status", JsonConvert.SerializeObject(updatedApplication.Status) },
                };

                await notificationService.Create(
                    NotificationType.Application,
                    NotificationAction.Update,
                    updatedApplication.Id,
                    this,
                    additionalData).ConfigureAwait(false);

                return mapper.Map<ApplicationDto>(updatedApplication);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError($"Updating failed. Exception = {ex.Message}.");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetNotificationsRecipients(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
        {
            var result = new List<User>();
            var applications = await applicationRepository.GetByFilter(a => a.Id == objectId, "Workshop.Provider.User").ConfigureAwait(false);

            if (action == NotificationAction.Create)
            {
                result.Add(applications.FirstOrDefault().Workshop.Provider.User);
            }
            else if (action == NotificationAction.Update)
            {
                if (additionalData.ContainsKey("Status")
                    && Enum.TryParse(additionalData["Status"], out ApplicationStatus applicationStatus))
                {
                    if (applicationStatus == ApplicationStatus.Approved)
                    {
                        result.Add(applications.FirstOrDefault().Parent.User);
                    }
                }
            }

            return result;
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
            var applications = applicationRepository.Get<int>(where: a => a.Id == id);

            if (!applications.Any())
            {
                logger.LogInformation($"Operation failed. Application with Id = {id} doesn't exist in the system.");
                throw new ArgumentException(localizer[$"Application with Id = {id} doesn't exist in the system."]);
            }
        }

        private async Task<bool> CheckChildParent(Guid parentId, Guid childId)
        {
            Expression<Func<Child, bool>> filter = c => c.ParentId == parentId;

            var children = childRepository.Get<int>(where: filter).Select(c => c.Id);

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

            var applications = await applicationRepository.GetByFilter(filter).ConfigureAwait(false);

            if (applications.Count() >= applicationsConstraintsConfig.ApplicationsLimit)
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

        private IQueryable<Application> GetFiltered(IQueryable<Application> applications, ApplicationFilter filter)
        {
            var filterPredicate = PredicateBuild(filter);
            var filteredApplications = applications.Where(filterPredicate);

            var sortPredicate = SortExpressionBuild(filter);
            filteredApplications = filteredApplications.DynamicOrderBy(sortPredicate);

            return filteredApplications;
        }

        private Expression<Func<Application, bool>> PredicateBuild(
            ApplicationFilter filter,
            Expression<Func<Application, bool>> predicate = null)
        {
            if (predicate is null)
            {
                predicate = PredicateBuilder.True<Application>();
            }

            if (filter.Status != 0)
            {
                predicate = predicate.And(a => a.Status == filter.Status);
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
    }
}
