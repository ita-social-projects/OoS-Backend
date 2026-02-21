using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for SocialGroup entity.
/// </summary>
/// <param name="repository">Repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
public class TagService(
    IEntityRepository<long, Tag> repository,
    ILogger<TagService> logger,
    IStringLocalizer<SharedResource> localizer
) : ITagService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<TagDto>> GetAll()
    {
        logger.LogDebug($"Getting all Tags, started.");

        var tags = await repository.GetAll().ConfigureAwait(false);

        logger.LogDebug(!tags.Any()
            ? "Tag table is empty."
            : $"All {tags.Count()} records were successfully received from the Tag table");

        return tags.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TagDto> GetById(long id)
    {
        logger.LogDebug($"Getting Tag by Id, started. Looking Id = {id}.");

        var tag = await repository.GetById(id).ConfigureAwait(false);

        if (tag == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["A Tag with a respective Id does not exist."]);
        }

        logger.LogDebug($"Successfully got a Tag with Id = {id}.");

        return tag.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TagDto> Create(TagCreate dto)
    {
        logger.LogDebug("Tag creating was started.");

        var tag = dto.ToModel();

        var newTag = await repository.Create(tag).ConfigureAwait(false);

        logger.LogDebug($"Tag with Id = {newTag?.Id} created successfully.");

        return newTag.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TagDto> Update(TagDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogDebug($"Updating Tag with Id = {dto.Id}, {localization} localization, started.");

        var tagLocalized = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (tagLocalized == null)
        {
            logger.LogError($"Updating failed. Tag with Id = {dto.Id} doesn't exist in the system.");

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

        logger.LogDebug($"Tag with Id = {tag.Id} updated succesfully.");

        return tag.ToDto();
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