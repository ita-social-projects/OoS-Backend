using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for InformationAboutPortal entity.
    /// </summary>
    public class InformationAboutPortalService : IInformationAboutPortalService
    {
        private readonly IEntityRepository<InformationAboutPortal> informationAboutPortalRepository;
        private readonly ILogger<InformationAboutPortalService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationAboutPortalService"/> class.
        /// </summary>
        /// <param name="informationAboutPortalRepository">InformationAboutPortal repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public InformationAboutPortalService(
            IEntityRepository<InformationAboutPortal> informationAboutPortalRepository,
            ILogger<InformationAboutPortalService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.informationAboutPortalRepository = informationAboutPortalRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> Create(InformationAboutPortalDto informationAboutPortalDto)
        {
            logger.LogDebug("InformationAboutPortal creating was started.");

            if (informationAboutPortalDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalDto));
            }

            if (await informationAboutPortalRepository.Count().ConfigureAwait(false) > 0)
            {
                throw new InvalidOperationException("Cannot create more than one record about portal");
            }

            Func<Task<InformationAboutPortal>> operation = async () =>
                await informationAboutPortalRepository.Create(mapper.Map<InformationAboutPortal>(informationAboutPortalDto)).ConfigureAwait(false);

            var newInformationAboutPortal = await informationAboutPortalRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"InformationAboutPortal with Id = {newInformationAboutPortal?.Id} created successfully.");

            return mapper.Map<InformationAboutPortalDto>(newInformationAboutPortal);
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> Update(InformationAboutPortalDto informationAboutPortalDto)
        {
            logger.LogDebug("Updating InformationAboutPortal started.");

            InformationAboutPortalDto updatedInformationAboutPortalDto;

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            if (!infoAboutPortals.Any())
            {
                updatedInformationAboutPortalDto = await Create(informationAboutPortalDto).ConfigureAwait(false);
            }
            else
            {
                InformationAboutPortal currentInformationAboutPortal = infoAboutPortals.Single();
                mapper.Map(informationAboutPortalDto, currentInformationAboutPortal);
                var informationAboutPortal = await informationAboutPortalRepository.Update(currentInformationAboutPortal).ConfigureAwait(false);
                updatedInformationAboutPortalDto = mapper.Map<InformationAboutPortalDto>(informationAboutPortal);
            }

            logger.LogDebug("Updating InformationAboutPortal finished.");

            return updatedInformationAboutPortalDto;
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> GetInformationAboutPortal()
        {
            logger.LogDebug("Get information about portal is started.");

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            var informationAboutPortal = infoAboutPortals.SingleOrDefault();

            logger.LogDebug("Get information about portal is finished.");

            return mapper.Map<InformationAboutPortalDto>(informationAboutPortal);
        }
    }
}
