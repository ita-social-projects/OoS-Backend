using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for SocialGroup entity.
/// </summary>
public class TagService : ITagService
{
    private readonly IEntityRepository<long, Tag> repository;
    private readonly ILogger<TagService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public TagService(
        IEntityRepository<long, Tag> repository,
        ILogger<TagService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TagDto>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogDebug($"Getting all Tags, {localization} localization, started.");

        var tags = await repository.GetAll().ConfigureAwait(false);

        logger.LogDebug(!tags.Any()
            ? "Tag table is empty."
            : $"All {tags.Count()} records were successfully received from the Tag table");

        return mapper.Map<List<TagDto>>(tags, opt =>
        opt.Items["Localization"] = localization);
    }

    /// <inheritdoc/>
    public async Task<TagDto> GetById(long id, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogDebug($"Getting Tag by Id, {localization} localization, started. Looking Id = {id}.");

        var tag = await repository.GetById(id).ConfigureAwait(false);

        if (tag == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogDebug($"Successfully got a SocialGroup with Id = {id} and {localization} localization.");

        return mapper.Map<TagDto>(tag, opt =>
        opt.Items["Localization"] = localization);
    }

    /// <inheritdoc/>
    public async Task<Tag> Create(TagCreate dto)
    {
        logger.LogDebug("Tag creating was started.");

        var tag = mapper.Map<Tag>(dto);

        var newTag = await repository.Create(tag).ConfigureAwait(false);

        logger.LogDebug($"Tag with Id = {newTag?.Id} created successfully.");

        return newTag;
    }

    /// <inheritdoc/>
    public async Task<Tag> Update(TagDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogDebug($"Updating Tag with Id = {dto?.Id}, {localization} localization, started.");

        var tagLocalized = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (tagLocalized == null)
        {
            logger.LogError($"Updating failed. Tag with Id = {dto?.Id} doesn't exist in the system.");

            return null;
        }

        if (localization == LocalizationType.En)
        {
            tagLocalized.NameEn = dto.Name;
        }
        else
        {
            tagLocalized.Name = dto.Name;
        }

        var tag = await repository.Update(tagLocalized).ConfigureAwait(false);

        logger.LogDebug($"Tag with Id = {tag?.Id} updated succesfully.");

        return tag;
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogDebug($"Deleting Tag with Id = {id} started.");

        var tag = await repository.GetById(id).ConfigureAwait(false);

        if (tag == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Tag with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(tag).ConfigureAwait(false);

        logger.LogDebug($"Tag with Id = {id} succesfully deleted.");
    }
}