using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IEntityRepository<Application> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        public ApplicationService(IEntityRepository<Application> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.logger = logger;
            this.localizer = localizer;
        }

        public async Task<ApplicationDTO> Create(ApplicationDTO applicationDTO)
        {
            logger.Information("Application creating was started.");

            var application = applicationDTO.ToDomain();

            var newApplication = await repository.Create(application).ConfigureAwait(false);

            logger.Information("Application created succesfully.");

            return newApplication.ToModel();
        }

        public async Task Delete(long id)
        {
            logger.Information("Application delete was launching.");

            var application = new Application { Id = id };

            try
            {
                await repository.Delete(application).ConfigureAwait(false);

                logger.Information("Application successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Application in the Db with such an id.");
                throw;
            }
        }

        public async Task<IEnumerable<ApplicationDTO>> GetAll()
        {
            logger.Information("Process of getting all Applications started.");

            var applications = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? "Application table is empty."
                : "Successfully got all records from the Application table.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        public async Task<IEnumerable<ApplicationDTO>> GetAllByUser(string id)
        {
            logger.Information("Process of getting Application by User Id started.");

            Expression<Func<Application, bool>> filter = a => a.UserId == id;

            var applications = await repository.GetByFilter(filter).ConfigureAwait(false);

            if (!applications.Any())
            {
                throw new ArgumentException(localizer["There is no Application in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got Applications with User id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        public async Task<IEnumerable<ApplicationDTO>> GetAllByWorkshop(long id)
        {
            logger.Information("Process of getting Application by Workshop Id started.");

            Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;

            var applications = await repository.GetByFilter(filter).ConfigureAwait(false);

            if (!applications.Any())
            {
                throw new ArgumentException(localizer["There is no Application in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got Applications with Workshop id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        public async Task<ApplicationDTO> GetById(long id)
        {
            logger.Information("Process of getting Application by id started.");

            var application = await repository.GetById(id).ConfigureAwait(false);

            if (application is null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got an Application with id = { id}.");

            return application.ToModel();
        }

        public async Task<ApplicationDTO> Update(ApplicationDTO dto)
        {
            logger.Information("Application updating was launched.");

            try
            {
                var application = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Application successfully updated.");

                return application.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed.There is no application in the Db with such an id.");
                throw;
            }
        }
    }
}
