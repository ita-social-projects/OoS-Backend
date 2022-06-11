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
    public class InstitutionService : IInstitutionService
    {
        private readonly ISensitiveEntityRepository<Institution> repository;
        private readonly ILogger<InstitutionService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public InstitutionService(
            ISensitiveEntityRepository<Institution> repository,
            ILogger<InstitutionService> logger,
            IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<List<InstitutionDto>> GetAll()
        {
            logger.LogInformation("Getting all Institutions started.");

            var institutions = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!institutions.Any()
                ? "Institution table is empty."
                : $"All {institutions.Count()} records were successfully received from the Institution table");

            return institutions.Select(institution => mapper.Map<InstitutionDto>(institution)).ToList();
        }
    }
}
