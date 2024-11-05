using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.SocialGroup;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for SocialGroup entity.
/// </summary>
public class SocialGroupService : ISocialGroupService
{
    private readonly IEntityRepositorySoftDeleted<long, SocialGroup> repository;
    private readonly ILogger<SocialGroupService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialGroupService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public SocialGroupService(
        IEntityRepositorySoftDeleted<long, SocialGroup> repository,
        ILogger<SocialGroupService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SocialGroupDto>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting all Social Groups, {localization} localization, started.");

        var socialGroups = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!socialGroups.Any()
            ? "SocialGroup table is empty."
            : $"All {socialGroups.Count()} records were successfully received from the SocialGroup table");

        return socialGroups.Select(x =>
        new SocialGroupDto()
        {
            Id = x.Id,
            Name = localization == LocalizationType.En ? x.NameEn : x.Name,
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> GetById(long id, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting SocialGroup by Id, {localization} localization, started. Looking Id = {id}.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a SocialGroup with Id = {id} and {localization} localization.");

        return new SocialGroupDto()
        {
            Id = socialGroup.Id,
            Name = localization == LocalizationType.En ? socialGroup.NameEn : socialGroup.Name,
        };

        /*return mapper.Map<SocialGroupDto>(socialGroup, opt =>
        opt.Items["Localization"] = localization);*/
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> Create(SocialGroupCreate dto)
    {
        logger.LogInformation("SocialGroup creating was started.");

        var socialGroup = mapper.Map<SocialGroup>(dto);

        var newSocialGroup = await repository.Create(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {newSocialGroup?.Id} created successfully.");

        return new SocialGroupDto()
        {
            Id = socialGroup.Id,
            Name = socialGroup.Name
        };
    }

    /// <inheritdoc/>
    public async Task<SocialGroupDto> Update(SocialGroupDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Updating SocialGroup with Id = {dto?.Id}, {localization} localization, started.");

        var socialGroupLocalized = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (socialGroupLocalized == null)
        {
            logger.LogError($"Updating failed. SocialGroup with Id = {dto?.Id} doesn't exist in the system.");

            return null;
        }

        if (localization == LocalizationType.En)
        {
            socialGroupLocalized.NameEn = dto.Name;
        }
        else
        {
            socialGroupLocalized.Name = dto.Name;
        }

        var socialGroup = await repository.Update(socialGroupLocalized).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {socialGroup?.Id} updated succesfully.");

        return new SocialGroupDto()
        {
            Id = socialGroup.Id,
            Name = localization == LocalizationType.En ? socialGroup.NameEn : socialGroup.Name,
        };
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting SocialGroup with Id = {id} started.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"SocialGroup with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {id} succesfully deleted.");
    }
}