using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for AboutPortal entity.
    /// </summary>
    public class AboutPortalService : IAboutPortalService
    {
        private const int LimitOfItems = 10;

        private readonly IAboutPortalRepository aboutPortalRepository;
        private readonly ILogger<AboutPortalService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPortalService"/> class.
        /// </summary>
        /// <param name="aboutPortalRepository">AboutPortal repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public AboutPortalService(
            IAboutPortalRepository aboutPortalRepository,
            ILogger<AboutPortalService> logger,
            IMapper mapper)
        {
            this.aboutPortalRepository = aboutPortalRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> Update(AboutPortalDto dto)
        {
            logger.LogDebug("Updating InformationAboutPortal started.");

            if (dto.AboutPortalItems.Count() > LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var aboutPortal = await aboutPortalRepository.GetWithNavigations().ConfigureAwait(false);

            if (aboutPortal == null)
            {
                aboutPortal = await Create(dto).ConfigureAwait(false);
            }

            aboutPortalRepository.DeleteAllItems();
            var items = mapper.Map<List<AboutPortalItem>>(dto.AboutPortalItems);
            aboutPortal.AboutPortalItems = items;

            await aboutPortalRepository.CreateItems(items).ConfigureAwait(false);

            try
            {
                await aboutPortalRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Updating failed. Exception: {exception.Message}");
                throw;
            }

            return mapper.Map<AboutPortalDto>(aboutPortal);
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> Get()
        {
            logger.LogDebug("Get information about portal is started.");

            var aboutPortal = await aboutPortalRepository.GetWithNavigations().ConfigureAwait(false);

            logger.LogDebug("Get information about portal is finished.");

            return mapper.Map<AboutPortalDto>(aboutPortal);
        }

        private async Task<AboutPortal> Create(AboutPortalDto dto)
        {
            logger.LogDebug("AboutPortal creating was started.");

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (await aboutPortalRepository.Count().ConfigureAwait(false) > 0)
            {
                throw new InvalidOperationException("Cannot create more than one record about portal.");
            }

            Func<Task<AboutPortal>> operation = async () =>
                await aboutPortalRepository.Create(mapper.Map<AboutPortal>(dto)).ConfigureAwait(false);

            var aboutPortal = await aboutPortalRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"InformationAboutPortal with Id = {aboutPortal?.Id} created successfully.");

            return aboutPortal;
        }
    }
}
