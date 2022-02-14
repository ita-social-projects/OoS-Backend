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
    /// Implements the interface with CRUD functionality for SupportInformation entity.
    /// </summary>
    public class SupportInformationService : ISupportInformationService
    {
        private readonly IEntityRepository<SupportInformation> supportInformationRepository;
        private readonly ILogger<SupportInformationService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportInformationService"/> class.
        /// </summary>
        /// <param name="supportInformationRepository">SupportInformation repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public SupportInformationService(
            IEntityRepository<SupportInformation> supportInformationRepository,
            ILogger<SupportInformationService> logger,
            IMapper mapper)
        {
            this.supportInformationRepository = supportInformationRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<SupportInformationDto> Create(SupportInformationDto supportInformationDto)
        {
            logger.LogDebug("SupportInformation creating was started.");

            if (supportInformationDto is null)
            {
                throw new ArgumentNullException(nameof(supportInformationDto));
            }

            if (await supportInformationRepository.Count().ConfigureAwait(false) > 0)
            {
                throw new InvalidOperationException("Cannot create more than one record about portal");
            }

            Func<Task<SupportInformation>> operation = async () =>
                await supportInformationRepository.Create(mapper.Map<SupportInformation>(supportInformationDto)).ConfigureAwait(false);

            var newSupportInformation = await supportInformationRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"ServiceInformation with Id = {newSupportInformation?.Id} created successfully.");

            return mapper.Map<SupportInformationDto>(newSupportInformation);
        }

        /// <inheritdoc/>
        public async Task<SupportInformationDto> GetSupportInformation()
        {
            logger.LogDebug("Get support information is started.");

            var supportInfos = await supportInformationRepository.GetAll().ConfigureAwait(false);
            var supportInformation = supportInfos.SingleOrDefault();

            logger.LogDebug("Get support information is finished.");

            return mapper.Map<SupportInformationDto>(supportInformation);
        }

        /// <inheritdoc/>
        public async Task<SupportInformationDto> Update(SupportInformationDto supportInformationDto)
        {
            logger.LogDebug("Updating SupportInformation started.");

            SupportInformationDto updatedSupportInformationDto;

            var supportInfos = await supportInformationRepository.GetAll().ConfigureAwait(false);

            if (!supportInfos.Any())
            {
                updatedSupportInformationDto = await Create(supportInformationDto).ConfigureAwait(false);
            }
            else
            {
                SupportInformation currentSupportInformation = supportInfos.Single();
                mapper.Map(supportInformationDto, currentSupportInformation);
                var supportInformation = await supportInformationRepository.Update(currentSupportInformation).ConfigureAwait(false);
                updatedSupportInformationDto = mapper.Map<SupportInformationDto>(supportInformation);
            }

            logger.LogDebug("Updating SupportInformation finished.");

            return updatedSupportInformationDto;
        }
    }
}
