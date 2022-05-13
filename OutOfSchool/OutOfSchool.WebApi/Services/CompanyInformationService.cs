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
        public async Task<CompanyInformationDto> Update(CompanyInformationDto companyInformationDto)
        {
            logger.LogDebug("Updating CompanyInformation started.");

            if (companyInformationDto.CompanyInformationItems.Count() > LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var companyInformation = await companyInformationRepository.GetWithNavigationsByTypeAsync(companyInformationDto.Type).ConfigureAwait(false);

            if (companyInformation == null)
            {
                companyInformation = await Create(companyInformationDto).ConfigureAwait(false);
            }
            else
            {
                companyInformationRepository.DeleteAllItemsByEntityAsync(companyInformation);

                companyInformation.Title = companyInformationDto.Title;
            }

            var items = mapper.Map<List<CompanyInformationItem>>(companyInformationDto.CompanyInformationItems);
            companyInformation.CompanyInformationItems = items;

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

            return mapper.Map<CompanyInformationDto>(companyInformation);
        }

        /// <inheritdoc/>
        public async Task<CompanyInformationDto> GetByType(CompanyInformationType type)
        {
            logger.LogDebug("Get CompanyInformation is started.");

            var companyInformation = await companyInformationRepository.GetWithNavigationsByTypeAsync(type).ConfigureAwait(false);

            logger.LogDebug("Get CompanyInformation is finished.");

            return mapper.Map<CompanyInformationDto>(companyInformation);
        }

        private async Task<CompanyInformation> Create(CompanyInformationDto companyInformationDto)
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

            Func<Task<CompanyInformation>> operation = async () =>
                await companyInformationRepository.Create(mapper.Map<CompanyInformation>(companyInformationDto)).ConfigureAwait(false);

            var aboutPortal = await companyInformationRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"CompanyInformation with Id = {aboutPortal?.Id} created successfully.");

            return aboutPortal;
        }
    }
}
