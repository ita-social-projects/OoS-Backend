using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Implements the interface with CRUD functionality for the Company Information entities (AboutPortal, SupportInformation and LawsAndRegulations).
    /// </summary>
    public class CompanyInformationService : ICompanyInformationService
    {
        private const int LimitOfItems = 10;

        private readonly ICompanyInformationRepository companyInformationRepository;
        private readonly ILogger<CompanyInformationService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyInformationService"/> class.
        /// </summary>
        /// <param name="companyInformationRepository">CompanyInformation repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public CompanyInformationService(
            ICompanyInformationRepository companyInformationRepository,
            ILogger<CompanyInformationService> logger,
            IMapper mapper)
        {
            this.companyInformationRepository = companyInformationRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> Update(AboutPortalDto companyInformationDto)
        {
            logger.LogDebug("Updating CompanyInformation started.");

            if (companyInformationDto.AboutPortalItems.Count() > LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var aboutPortal = await companyInformationRepository.GetWithNavigationsByTypeAsync(companyInformationDto.Type).ConfigureAwait(false);

            if (aboutPortal == null)
            {
                aboutPortal = await Create(companyInformationDto).ConfigureAwait(false);
            }
            else
            {
                companyInformationRepository.DeleteAllItemsByEntityAsync(aboutPortal);
            }

            var items = mapper.Map<List<AboutPortalItem>>(companyInformationDto.AboutPortalItems);
            aboutPortal.AboutPortalItems = items;

            await companyInformationRepository.CreateItems(items).ConfigureAwait(false);

            try
            {
                await companyInformationRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Updating CompanyInformation failed. Exception: {exception.Message}");
                throw;
            }

            return mapper.Map<AboutPortalDto>(aboutPortal);
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> GetByType(CompanyInformationType type)
        {
            logger.LogDebug("Get CompanyInformation is started.");

            var aboutPortal = await companyInformationRepository.GetWithNavigationsByTypeAsync(type).ConfigureAwait(false);

            logger.LogDebug("Get CompanyInformation is finished.");

            return mapper.Map<AboutPortalDto>(aboutPortal);
        }

        private async Task<AboutPortal> Create(AboutPortalDto companyInformationDto)
        {
            logger.LogDebug("CompanyInformation creating was started.");

            if (companyInformationDto == null)
            {
                throw new ArgumentNullException(nameof(companyInformationDto));
            }

            if (await companyInformationRepository.Count().ConfigureAwait(false) > 0)
            {
                throw new InvalidOperationException("Cannot create more than one record about portal.");
            }

            Func<Task<AboutPortal>> operation = async () =>
                await companyInformationRepository.Create(mapper.Map<AboutPortal>(companyInformationDto)).ConfigureAwait(false);

            var aboutPortal = await companyInformationRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"CompanyInformation with Id = {aboutPortal?.Id} created successfully.");

            return aboutPortal;
        }
    }
}
