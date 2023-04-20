using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Nest;
using OutOfSchool.Redis;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.Application;
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
    private readonly IRegionAdminService regionAdminService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cache">Redis cache service.</param>
    /// <param name="currentUserService">Service for manage current user.</param>
    /// <param name="ministryAdminService">Service for manage ministry admin</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    public InstitutionService(
        ISensitiveEntityRepository<Institution> repository,
        ILogger<InstitutionService> logger,
        IMapper mapper,
        ICacheService cache,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAll(bool filterNonGovernment)
    {
        logger.LogInformation("Getting all Institutions started");

        string cacheKey = "InstitutionService_GetAll";

        var institutions = await cache.GetOrAddAsync(cacheKey, GetAllFromDatabase);

        var filterPredicate = await PredicateBuild(filterNonGovernment);
        institutions = institutions.Where(filterPredicate).ToList();

        logger.LogInformation(institutions.Count == 0
            ? "Institution table is empty."
            : $"All {institutions.Count} records were successfully received from the Institution table");

        return institutions;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionDto>> GetAllFromDatabase()
    {
        var institutions = await repository.GetAll().ConfigureAwait(false);
        return institutions.Select(institution => mapper.Map<InstitutionDto>(institution)).ToList();
    }

    private async Task<Func<InstitutionDto, bool>> PredicateBuild(bool filterNonGovernment)
    {
        var predicate = PredicateBuilder.True<InstitutionDto>();

        predicate = filterNonGovernment ? predicate.And(x => x.IsGovernment) : predicate;

        Guid filteredInstitutionId = Guid.Empty;

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            filteredInstitutionId = ministryAdmin.InstitutionId;
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            filteredInstitutionId = regionAdmin.InstitutionId;
        }

        predicate = filteredInstitutionId.Equals(Guid.Empty) ? predicate : predicate.And(i => i.Id == filteredInstitutionId);

        return predicate.Compile();
    }
}