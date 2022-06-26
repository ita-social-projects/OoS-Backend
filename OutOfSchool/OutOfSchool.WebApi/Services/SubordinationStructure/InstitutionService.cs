using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OutOfSchool.Redis;
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
        private readonly ICacheService cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        /// <param name="cache">Redis cache service.</param>
        public InstitutionService(
            ISensitiveEntityRepository<Institution> repository,
            ILogger<InstitutionService> logger,
            IMapper mapper,
            ICacheService cache)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc/>
        public async Task<List<InstitutionDto>> GetAll()
        {
            logger.LogInformation("Getting all Institutions started.");

            string cacheKey = "InstitutionService_GetAll";

            var institutions = await cache.GetOrAddAsync(cacheKey, () =>
                GetAllFromDatabase()).ConfigureAwait(false);

            return institutions;
        }

        /// <inheritdoc/>
        public async Task<List<InstitutionDto>> GetAllFromDatabase()
        {
            var institutions = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!institutions.Any()
                ? "Institution table is empty."
                : $"All {institutions.Count()} records were successfully received from the Institution table");

            return institutions.Select(institution => mapper.Map<InstitutionDto>(institution)).ToList();
        }
    }
}
