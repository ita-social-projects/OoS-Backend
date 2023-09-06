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

namespace OutOfSchool.WebApi.Services.SubordinationStructure;

public class InstitutionFieldDescriptionService : IInstitutionFieldDescriptionService
{
    private readonly ISensitiveEntityRepositorySoftDeleted<InstitutionFieldDescription> repository;
    private readonly ILogger<InstitutionFieldDescriptionService> logger;
    private readonly IMapper mapper;
    private readonly ICacheService cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionFieldDescriptionService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cache">Redis cache service.</param>
    public InstitutionFieldDescriptionService(
        ISensitiveEntityRepositorySoftDeleted<InstitutionFieldDescription> repository,
        ILogger<InstitutionFieldDescriptionService> logger,
        IMapper mapper,
        ICacheService cache)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionId(Guid id)
    {
        logger.LogInformation("Getting all entities InstitutionFieldDescription by Institution id started.");

        string cacheKey = $"InstitutionFieldDescriptionService_GetByInstitutionId_{id}";

        var institutionFieldDescriptions = await cache.GetOrAddAsync(cacheKey, () =>
            GetByInstitutionIdFromDatabase(id)).ConfigureAwait(false);

        return institutionFieldDescriptions;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionFieldDescriptionDto>> GetByInstitutionIdFromDatabase(Guid id)
    {
        var institutionFieldDescriptions = await repository.GetByFilter(i => i.InstitutionId == id).ConfigureAwait(false);

        logger.LogInformation(!institutionFieldDescriptions.Any()
            ? $"There is no desriptions in InstitutionFieldDescription table for id = {id}."
            : $"{institutionFieldDescriptions.Count()} records were successfully received from the InstitutionFieldDescription table for id = {id}.");

        return institutionFieldDescriptions.Select(entity => mapper.Map<InstitutionFieldDescriptionDto>(entity)).ToList();
    }
}