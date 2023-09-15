using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for InstitutionStatus entity.
/// </summary>
public class StatusService : IStatusService
{

    private readonly IEntityRepositorySoftDeleted<long, InstitutionStatus> repository;
    private readonly ILogger<StatusService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public StatusService(
        IEntityRepositorySoftDeleted<long, InstitutionStatus> repository,
        ILogger<StatusService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }


    /// <inheritdoc/>
    public async Task<IEnumerable<InstitutionStatusDTO>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting all Institution Statuses, {localization} localization, started.");

        var institutionStatuses = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!institutionStatuses.Any()
            ? "InstitutionStatus table is empty."
            : $"All {institutionStatuses.Count()} records were successfully received from the InstitutionStatus table");

        return institutionStatuses.Select(institutionStatus => new InstitutionStatusDTO
        {
            Id = institutionStatus.Id,
            Name = localization == LocalizationType.En ? institutionStatus.NameEn : institutionStatus.Name,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<InstitutionStatusDTO> GetById(long id, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting InstitutionStatus by Id started, {localization} localization. Looking Id = {id}.");

        var institutionStatus = await repository.GetById(id).ConfigureAwait(false);

        if (institutionStatus is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a institutionStatus with Id = {id}.");

        return new InstitutionStatusDTO
        {
            Id = institutionStatus.Id,
            Name = localization == LocalizationType.En ? institutionStatus.NameEn : institutionStatus.Name,
        };
    }

    /// <inheritdoc/>
    public async Task<InstitutionStatusDTO> Create(InstitutionStatusDTO dto)
    {
        logger.LogInformation("InstitutionStatus creating was started.");

        var institutionStatus = mapper.Map<InstitutionStatus>(dto);

        var newInstitutionStatus = await repository.Create(institutionStatus).ConfigureAwait(false);

        logger.LogInformation($"InstitutionStatus with Id = {newInstitutionStatus?.Id} created successfully.");

        return mapper.Map<InstitutionStatusDTO>(newInstitutionStatus);
    }

    /// <inheritdoc/>
    public async Task<InstitutionStatusDTO> Update(InstitutionStatusDTO dto, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Updating InstitutionStatus with Id = {dto?.Id}, {localization} localization started.");

        ArgumentNullException.ThrowIfNull(dto);

        var institutionStatusLocalized = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (institutionStatusLocalized is null)
        {
            logger.LogError($"Updating failed. InstitutionStatus with Id = {dto.Id} doesn't exist in the system.");
            return null;
        }

        if (localization == LocalizationType.En)
        {
            institutionStatusLocalized.NameEn = dto.Name;
        }
        else
        {
            institutionStatusLocalized.Name = dto.Name;
        }

        var institutionStatus = await repository.Update(institutionStatusLocalized).ConfigureAwait(false);

        logger.LogInformation($"InstitutionStatus with Id = {institutionStatus?.Id} updated succesfully.");

        return new InstitutionStatusDTO
        {
            Id = institutionStatus.Id,
            Name = localization == LocalizationType.En ? institutionStatus.NameEn : institutionStatus.Name,
        };
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting InstitutionStatus with Id = {id} started.");

        var institutionStatus = await repository.GetById(id).ConfigureAwait(false);

        if (institutionStatus == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"InstitutionStatus with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(institutionStatus).ConfigureAwait(false);

        logger.LogInformation($"InstitutionStatus with Id = {id} succesfully deleted.");
    }
}