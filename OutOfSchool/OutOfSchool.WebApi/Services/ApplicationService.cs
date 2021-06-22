using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Application entity.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository applicationRepository;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IEntityRepository<Child> childRepository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationService"/> class.
        /// </summary>
        /// <param name="repository">Application repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="workshopRepository">Workshop repository.</param>
        /// <param name="childRepository">Child repository.</param>
        public ApplicationService(
            IApplicationRepository repository, 
            ILogger logger, 
            IStringLocalizer<SharedResource> localizer,
            IWorkshopRepository workshopRepository,
            IEntityRepository<Child> childRepository)
        {
            this.applicationRepository = repository;
            this.workshopRepository = workshopRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.childRepository = childRepository;
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> Create(ApplicationDto applicationDto)
        {
            logger.Information("Application creating started.");

            ModelCreationValidation(applicationDto);

            var isChildParent = await CheckChildParent(applicationDto.ParentId, applicationDto.ChildId).ConfigureAwait(false);

            if (!isChildParent)
            {
                logger.Information("Operation failed. Unable to create application for another parent`s child.");
                throw new ArgumentException(localizer["Unable to create application for another parent`s child."]);
            }

            var application = applicationDto.ToDomain();

            var newApplication = await applicationRepository.Create(application).ConfigureAwait(false);

            logger.Information($"Application with Id = {newApplication?.Id} created successfully.");

            return newApplication.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> Create(IEnumerable<ApplicationDto> applicationDtos)
        {
            logger.Information("Multiple applications creating started.");

            MultipleModelCreationValidation(applicationDtos);

            var applications = applicationDtos.Select(a => a.ToDomain()).ToList();

            var newApplications = await applicationRepository.Create(applications).ConfigureAwait(false);

            logger.Information("Applications created successfully.");

            return newApplications.Select(a => a.ToModel());
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Application with Id = {id} started.");

            CheckApplicationExists(id);

            var application = new Application { Id = id };

            try
            {
                await applicationRepository.Delete(application).ConfigureAwait(false);

                logger.Information($"Application with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"Deleting failed. Exception: {ex.Message}.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAll()
        {
            logger.Information("Getting all Applications started.");

            var applications = await applicationRepository.GetAllWithDetails("Workshop,Child,Parent").ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? "Application table is empty."
                : $"All {applications.Count()} records were successfully received from the Application table");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByParent(long id)
        {
            logger.Information($"Getting Applications by Parent Id started. Looking Parent Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.ParentId == id;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? $"There is no applications in the Db with Parent Id = {id}."
                : $"Successfully got Applications with Parent Id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByWorkshop(long id)
        {
            logger.Information($"Getting Applications by Workshop Id started. Looking Workshop Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? $"There is no applications in the Db with Workshop Id = {id}."
                : $"Successfully got Applications with Workshop Id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByProvider(long id)
        {
            logger.Information($"Getting Applications by Provider Id started. Looking Provider Id = {id}.");

            Expression<Func<Workshop, bool>> workshopFilter = w => w.ProviderId == id;

            var workshops = workshopRepository.Get<int>(where: workshopFilter).Select(w => w.Id);

            Expression<Func<Application, bool>> applicationFilter = a => workshops.Contains(a.WorkshopId);

            var applications = await applicationRepository.GetByFilter(applicationFilter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? $"There is no applications in the Db with Provider Id = {id}."
                : $"Successfully got Applications with Provider Id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByStatus(int status)
        {
            logger.Information($"Getting Applications by Status started. Looking Status = {status}.");

            Expression<Func<Application, bool>> filter = a => (int)a.Status == status;

            var applications = await applicationRepository.GetByFilter(filter, "Workshop,Child,Parent").ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? $"There is no applications in the Db with Status = {status}."
                : $"Successfully got Applications with Status = {status}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> GetById(long id)
        {
            logger.Information($"Getting Application by Id started. Looking Id = {id}.");

            Expression<Func<Application, bool>> filter = a => a.Id == id;

            var application = await applicationRepository.Get<int>(where: filter, includeProperties: "Workshop,Child,Parent")
                                              .FirstOrDefaultAsync().ConfigureAwait(false);

            if (application is null)
            {
                logger.Information($"There is no application in the Db with Id = {id}.");
                return null;
            }

            logger.Information($"Successfully got an Application with Id = {id}.");

            return application.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> Update(ShortApplicationDTO applicationDto)
        {
            logger.Information($"Updating Application with Id = {applicationDto?.Id} started.");

            CheckApplicationExists(applicationDto.Id);

            try
            {
                var application = await applicationRepository.GetById(applicationDto.Id).ConfigureAwait(false);

                application.Status = applicationDto.Status;

                var updatedApplication = await applicationRepository.Update(application).ConfigureAwait(false);

                logger.Information($"Application with Id = {applicationDto?.Id} updated succesfully.");

                return updatedApplication.ToModel();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"Updating failed. Exception = {ex.Message}.");
                throw;
            }
        }

        private void ModelNullValidation(ApplicationDto applicationDto)
        {
            if (applicationDto is null)
            {
                logger.Information("Operation failed. ApplicationDto is null");
                throw new ArgumentException(localizer["Application dto is null."], nameof(applicationDto));
            }
        }

        private void ModelCreationValidation(ApplicationDto applicationDto)
        {
            ModelNullValidation(applicationDto);

            Expression<Func<Application, bool>> filter = a => a.ChildId == applicationDto.ChildId
                                                              && a.WorkshopId == applicationDto.WorkshopId
                                                              && a.ParentId == applicationDto.ParentId;
            if (applicationRepository.Get<int>(where: filter).Any())
            {
                logger.Information("Creation failed. Application with such data alredy exists.");
                throw new ArgumentException(localizer["There is already an application with such data."]);
            }
        }

        private void MultipleModelCreationValidation(IEnumerable<ApplicationDto> applicationDtos)
        {
            if (!applicationDtos.Any())
            {
                logger.Information("Operation failed. There is no application to create.");
                throw new ArgumentException(localizer["There is no application to create."]);
            }

            foreach (var application in applicationDtos)
            {
                ModelCreationValidation(application);
            }
        }

        private void CheckApplicationExists(long id)
        {
            var applications = applicationRepository.Get<int>(where: a => a.Id == id);

            if (!applications.Any())
            {
                logger.Information($"Operation failed. Application with Id = {id} doesn't exist in the system.");
                throw new ArgumentException(localizer[$"Application with Id = {id} doesn't exist in the system."]);
            }
        }

        private async Task<bool> CheckChildParent(long parentId, long childId)
        {
            Expression<Func<Child, bool>> filter = c => c.ParentId == parentId;

            var children = childRepository.Get<int>(where: filter).Select(c => c.Id);

            return await children.ContainsAsync(childId).ConfigureAwait(false);
        }
    }
}
