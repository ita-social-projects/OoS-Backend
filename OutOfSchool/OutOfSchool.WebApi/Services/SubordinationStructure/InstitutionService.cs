using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using OutOfSchool.Redis;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure;

public class InstitutionService : IInstitutionService
{
    private readonly ISensitiveEntityRepository<Institution> repository;
    private readonly ILogger<InstitutionService> logger;
    private readonly IMapper mapper;
    private readonly ICacheService cache;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cache">Redis cache service.</param>
    /// <param name="currentUserService">Service for manage current user.</param>
    /// <param name="ministryAdminService">Service for manage ministry admin</param>
    public InstitutionService(
        ISensitiveEntityRepository<Institution> repository,
        ILogger<InstitutionService> logger,
        IMapper mapper,
        ICacheService cache,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAll(bool filterNonGovernment)
    {
        logger.LogInformation("Getting all Institutions started");

        string cacheKey = "InstitutionService_GetAll";

        var institutions = await cache.GetOrAddAsync(cacheKey, GetAllFromDatabase);

        return !filterNonGovernment ? institutions : institutions.Where(i => !i.Title.Equals("Інше", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAllFromDatabase()
    {
        var institutions = await repository.GetAll().ConfigureAwait(false);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            institutions = institutions.Where(i => i.Id == ministryAdmin.InstitutionId);
        }

        logger.LogInformation(!institutions.Any()
            ? "Institution table is empty."
            : $"All {institutions.Count()} records were successfully received from the Institution table");

        return institutions.Select(institution => mapper.Map<InstitutionDto>(institution)).ToList();
    }
}