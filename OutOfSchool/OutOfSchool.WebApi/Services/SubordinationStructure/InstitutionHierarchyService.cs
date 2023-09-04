﻿using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.SubordinationStructure;

namespace OutOfSchool.WebApi.Services.SubordinationStructure;

public class InstitutionHierarchyService : IInstitutionHierarchyService
{
    private readonly IInstitutionHierarchyRepository repository;
    private readonly IWorkshopRepository repositoryWorkshop;
    private readonly IProviderRepository repositoryProvider;
    private readonly ILogger<InstitutionHierarchyService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly ICacheService cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionHierarchyService"/> class.
    /// </summary>
    /// <param name="repository">Repository for InstitutionHierarchy entity.</param>
    /// <param name="repositoryWorkshop">Workshop repository.</param>
    /// <param name="repositoryProvider">Provider repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cache">Redis cache service.</param>
    public InstitutionHierarchyService(
        IInstitutionHierarchyRepository repository,
        IWorkshopRepository repositoryWorkshop,
        IProviderRepository repositoryProvider,
        ILogger<InstitutionHierarchyService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        ICacheService cache)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.repositoryWorkshop = repositoryWorkshop ?? throw new ArgumentNullException(nameof(repositoryWorkshop));
        this.repositoryProvider = repositoryProvider ?? throw new ArgumentNullException(nameof(repositoryProvider));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
    public async Task<List<InstitutionHierarchyDto>> GetAll()
    {
        logger.LogInformation("Getting all InstitutionHierarchies started.");

        var institutionHierarchies = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!institutionHierarchies.Any()
            ? "InstitutionHierarchy table is empty."
            : $"All {institutionHierarchies.Count()} records were successfully received from the InstitutionHierarchy table.");

        return institutionHierarchies.Select(entity => mapper.Map<InstitutionHierarchyDto>(entity)).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetChildren(Guid? parentId)
    {
        logger.LogInformation("Getting all children InstitutionHierarchies started.");

        string cacheKey = $"InstitutionHierarchyService_GetChildren_{parentId}";

        var institutionHierarchies = await cache.GetOrAddAsync(cacheKey, () =>
            GetChildrenFromDatabase(parentId)).ConfigureAwait(false);

        return institutionHierarchies;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetChildrenFromDatabase(Guid? parentId)
    {
        var institutionHierarchies = await repository.GetByFilter(i => i.ParentId == parentId).ConfigureAwait(false);

        logger.LogInformation(!institutionHierarchies.Any()
            ? $"There is no children in InstitutionHierarchy table for parentId = {parentId}."
            : $"{institutionHierarchies.Count()} records were successfully received from the InstitutionHierarchy table for parentId = {parentId}.");

        return institutionHierarchies.OrderBy(entity => entity.Title).Select(entity => mapper.Map<InstitutionHierarchyDto>(entity)).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetParents(Guid childId, bool includeCurrentLevel)
    {
        logger.LogInformation("Getting all parents InstitutionHierarchies started.");

        string cacheKey = $"InstitutionHierarchyService_GetParents_{childId}";

        var institutionHierarchies = await cache.GetOrAddAsync(cacheKey, () =>
            GetParentsFromDatabase(childId, includeCurrentLevel)).ConfigureAwait(false);

        return institutionHierarchies;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetParentsFromDatabase(Guid childId, bool includeCurrentLevel)
    {
        IEnumerable<InstitutionHierarchy> institutionHierarchiesTmp;
        var result = new List<InstitutionHierarchyDto>();
        var currentInstitutionHierarchy = await repository.GetById(childId).ConfigureAwait(false);

        if (currentInstitutionHierarchy != null)
        {
            if (includeCurrentLevel)
            {
                result.Add(mapper.Map<InstitutionHierarchyDto>(currentInstitutionHierarchy));
            }

            int amountOfLevels = currentInstitutionHierarchy.HierarchyLevel - 1;

            for (int i = amountOfLevels; i > 0; i--)
            {
                institutionHierarchiesTmp = await repository.GetByFilter(i => i.Id == currentInstitutionHierarchy.ParentId).ConfigureAwait(false);
                currentInstitutionHierarchy = institutionHierarchiesTmp.FirstOrDefault();

                if (currentInstitutionHierarchy is null)
                {
                    break;
                }

                result.Add(mapper.Map<InstitutionHierarchyDto>(currentInstitutionHierarchy));
            }
        }

        logger.LogInformation(!result.Any()
            ? $"There is no parents in InstitutionHierarchy table for childId = {childId}."
            : $"{result.Count} records were successfully received from the InstitutionHierarchy table for childId = {childId}.");

        return result;
    }

    /// <inheritdoc/>
    public async Task<InstitutionHierarchyDto> GetById(Guid id)
    {
        logger.LogInformation($"Getting InstitutionHierarchy by Id started. Looking Id = {id}.");

        var institutionHierarchy = await repository.GetById(id).ConfigureAwait(false);

        logger.LogInformation($"Successfully got a InstitutionHierarchy with Id = {id}.");

        return mapper.Map<InstitutionHierarchyDto>(institutionHierarchy);
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetAllByInstitutionAndLevel(Guid institutionId, int hierarchyLevel)
    {
        logger.LogInformation("Getting all InstitutionHierarchy objects by institution id and level started.");

        string cacheKey = $"InstitutionHierarchyService_GetAllByInstitutionAndLevel_{institutionId}_{hierarchyLevel}";

        var institutionHierarchies = await cache.GetOrAddAsync(cacheKey, () =>
            GetAllByInstitutionAndLevelFromDatabase(institutionId, hierarchyLevel)).ConfigureAwait(false);

        return institutionHierarchies;
    }

    /// <inheritdoc/>
    public async Task<List<InstitutionHierarchyDto>> GetAllByInstitutionAndLevelFromDatabase(Guid institutionId, int hierarchyLevel)
    {
        var institutionHierarchies = await repository.GetByFilter(
            i => i.InstitutionId == institutionId
                 && i.HierarchyLevel == hierarchyLevel).ConfigureAwait(false);

        logger.LogInformation(!institutionHierarchies.Any()
            ? $"There is no entities in InstitutionHierarchy table for institutionId = {institutionId} and hierarchyLevel = {hierarchyLevel}."
            : $"{institutionHierarchies.Count()} records were successfully received from the InstitutionHierarchy table for institutionId = {institutionId} and hierarchyLevel = {hierarchyLevel}.");

        return institutionHierarchies.OrderBy(entity => entity.Title).Select(entity => mapper.Map<InstitutionHierarchyDto>(entity)).ToList();
    }

    /// <inheritdoc/>
    public async Task<InstitutionHierarchyDto> Update(InstitutionHierarchyDto dto)
    {
        logger.LogDebug("Updating InstitutionHierarchy is started.");

        ArgumentNullException.ThrowIfNull(dto);

        var institutionHierarchy = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (institutionHierarchy is null)
        {
            var message = $"Updating failed. InstitutionHierarchy with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        mapper.Map(dto, institutionHierarchy);

        institutionHierarchy = await repository
            .Update(
                institutionHierarchy,
                dto.Directions.Select(d => d.Id).ToList())
            .ConfigureAwait(false);

        logger.LogInformation($"InstitutionHierarchy with Id = {institutionHierarchy?.Id} updated succesfully.");

        return mapper.Map<InstitutionHierarchyDto>(institutionHierarchy);
    }
}