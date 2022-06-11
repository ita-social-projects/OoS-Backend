using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure
{
    public class InstitutionFieldDescriptionService : IInstitutionFieldDescriptionService
    {
        private readonly ISensitiveEntityRepository<InstitutionFieldDescription> repository;
        private readonly ILogger<InstitutionFieldDescriptionService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionFieldDescriptionService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public InstitutionFieldDescriptionService(
            ISensitiveEntityRepository<InstitutionFieldDescription> repository,
            ILogger<InstitutionFieldDescriptionService> logger,
            IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionId(Guid id)
        {
            logger.LogInformation("Getting all entities InstitutionFieldDescription by Institution id started.");

            var institutionFieldDescriptions = await repository.GetByFilter(i => i.InstitutionId == id).ConfigureAwait(false);

            logger.LogInformation(!institutionFieldDescriptions.Any()
                ? $"There is no desriptions in InstitutionFieldDescription table for id = {id}."
                : $"{institutionFieldDescriptions.Count()} records were successfully received from the InstitutionFieldDescription table for id = {id}.");

            return institutionFieldDescriptions.Select(entity => mapper.Map<InstitutionFieldDescriptionDto>(entity)).ToList();
        }
    }
}
