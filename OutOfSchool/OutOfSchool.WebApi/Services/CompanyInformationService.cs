using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for the CompanyInformation entities (AboutPortal, SupportInformation and LawsAndRegulations).
    /// </summary>
    public class CompanyInformationService : ICompanyInformationService
    {
        private const int LimitOfItems = 10;

        private readonly IEntityRepository<CompanyInformation> companyInformationRepository;
        private readonly ILogger<CompanyInformationService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyInformationService"/> class.
        /// </summary>
        /// <param name="companyInformationRepository">CompanyInformation repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public CompanyInformationService(
            IEntityRepository<CompanyInformation> companyInformationRepository,
            ILogger<CompanyInformationService> logger,
            IMapper mapper)
        {
            this.companyInformationRepository = companyInformationRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<CompanyInformationDto> GetByType(CompanyInformationType type)
        {
            logger.LogDebug("Get CompanyInformation is started.");

            var companyInformation = (await companyInformationRepository.GetByFilter(GetFilter(type), "CompanyInformationItems").ConfigureAwait(false)).FirstOrDefault();

            logger.LogDebug("Get CompanyInformation is finished.");

            return mapper.Map<CompanyInformationDto>(companyInformation);
        }

        /// <inheritdoc/>
        public async Task<CompanyInformationDto> Update(CompanyInformationDto companyInformationDto)
        {
            logger.LogDebug("Updating CompanyInformation is started.");

            if (companyInformationDto.CompanyInformationItems.Count() > LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var companyInformation = (await companyInformationRepository.GetByFilter(GetFilter(companyInformationDto.Type), "CompanyInformationItems").ConfigureAwait(false)).FirstOrDefault();

            if (companyInformation == null)
            {
                companyInformation = await Create(companyInformationDto).ConfigureAwait(false);
            }
            else
            {
                var items = mapper.Map<List<CompanyInformationItem>>(companyInformationDto.CompanyInformationItems);

                companyInformation.CompanyInformationItems = items;
                companyInformation.Title = companyInformationDto.Title;
            }

            try
            {
                await companyInformationRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Updating CompanyInformation failed. Exception: {exception.Message}");
                throw;
            }

            logger.LogDebug("Updating CompanyInformation is finished.");

            return mapper.Map<CompanyInformationDto>(companyInformation);
        }

        private async Task<CompanyInformation> Create(CompanyInformationDto companyInformationDto)
        {
            logger.LogDebug("CompanyInformation creating is started.");

            if (companyInformationDto == null)
            {
                throw new ArgumentNullException(nameof(companyInformationDto));
            }

            var companyInformation = await companyInformationRepository.Create(mapper.Map<CompanyInformation>(companyInformationDto)).ConfigureAwait(false);

            logger.LogDebug($"CompanyInformation with Id = {companyInformation?.Id} created successfully.");

            return companyInformation;
        }

        private Expression<Func<CompanyInformation, bool>> GetFilter(CompanyInformationType type)
        {
            return ci => ci.Type == type;
        }
    }
}
