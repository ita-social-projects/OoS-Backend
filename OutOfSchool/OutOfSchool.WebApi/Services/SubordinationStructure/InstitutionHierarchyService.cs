using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure
{
    public class InstitutionHierarchyService : IInstitutionHierarchyService
    {
        private readonly ISensitiveEntityRepository<InstitutionHierarchy> repository;
        private readonly IWorkshopRepository repositoryWorkshop;
        private readonly IProviderRepository repositoryProvider;
        private readonly ILogger<InstitutionHierarchyService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionHierarchyService"/> class.
        /// </summary>
        /// <param name="repository">Repository for InstitutionHierarchy entity.</param>
        /// <param name="repositoryWorkshop">Workshop repository.</param>
        /// <param name="repositoryProvider">Provider repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public InstitutionHierarchyService(
            ISensitiveEntityRepository<InstitutionHierarchy> repository,
            IWorkshopRepository repositoryWorkshop,
            IProviderRepository repositoryProvider,
            ILogger<InstitutionHierarchyService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.repositoryWorkshop = repositoryWorkshop;
            this.repositoryProvider = repositoryProvider;
            this.localizer = localizer;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<InstitutionHierarchyDto> Create(InstitutionHierarchyDto dto)
        {
            logger.LogInformation("InstitutionHierarchy creating was started.");

            var institutionHierarchy = mapper.Map<InstitutionHierarchy>(dto);

            var newInstitutionHierarchy = await repository.Create(institutionHierarchy).ConfigureAwait(false);

            logger.LogInformation($"InstitutionHierarchy with Id = {newInstitutionHierarchy?.Id} created successfully.");

            return mapper.Map<InstitutionHierarchyDto>(newInstitutionHierarchy);
        }

        /// <inheritdoc/>
        public async Task<Result<InstitutionHierarchyDto>> Delete(Guid id)
        {
            logger.LogInformation($"Deleting InstitutionHierarchy with Id = {id} started.");

            var entity = new InstitutionHierarchy() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"InstitutionHierarchy with Id = {id} succesfully deleted.");

                return Result<InstitutionHierarchyDto>.Success(mapper.Map<InstitutionHierarchyDto>(entity));
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. InstitutionHierarchy with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InstitutionHierarchyDto>> GetAll()
        {
            logger.LogInformation("Getting all InstitutionHierarchies started.");

            var institutionHierarchies = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!institutionHierarchies.Any()
                ? "InstitutionHierarchy table is empty."
                : $"All {institutionHierarchies.Count()} records were successfully received from the InstitutionHierarchy table.");

            return institutionHierarchies.Select(entity => mapper.Map<InstitutionHierarchyDto>(entity)).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InstitutionHierarchyDto>> GetChildren(Guid? parentId)
        {
            logger.LogInformation("Getting all children InstitutionHierarchies started.");

            var institutionHierarchies = await repository.GetByFilter(i => i.ParentId == parentId).ConfigureAwait(false);

            logger.LogInformation(!institutionHierarchies.Any()
                ? $"There is no children in InstitutionHierarchy table for parentId = {parentId}."
                : $"{institutionHierarchies.Count()} records were successfully received from the InstitutionHierarchy table for parentId = {parentId}.");

            return institutionHierarchies.Select(entity => mapper.Map<InstitutionHierarchyDto>(entity)).ToList();
        }

        /// <inheritdoc/>
        public async Task<InstitutionHierarchyDto> GetById(Guid id)
        {
            logger.LogInformation($"Getting InstitutionHierarchy by Id started. Looking Id = {id}.");

            var institutionHierarchy = await repository.GetById(id).ConfigureAwait(false);

            logger.LogInformation($"Successfully got a Direction with Id = {id}.");

            return mapper.Map<InstitutionHierarchyDto>(institutionHierarchy);
        }

        /// <inheritdoc/>
        public async Task<InstitutionHierarchyDto> Update(InstitutionHierarchyDto dto)
        {
            logger.LogDebug("Updating InstitutionHierarchy is started.");

            try
            {
                var institutionHierarchy = await repository.Update(mapper.Map<InstitutionHierarchy>(dto)).ConfigureAwait(false);

                logger.LogInformation($"InstitutionHierarchy with Id = {institutionHierarchy?.Id} updated succesfully.");

                return mapper.Map<InstitutionHierarchyDto>(institutionHierarchy);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. InstitutionHierarchy with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }
    }
}
