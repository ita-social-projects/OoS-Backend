using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class SubjectService : ISubjectService
{
    private readonly IEntityRepository<long, StudySubject> repository;
    private readonly ILogger<SubjectService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubjectService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public SubjectService(
        IEntityRepository<long, StudySubject> repository,
        ILogger<SubjectService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> Create(StudySubjectDto dto)
    {
        logger.LogDebug("StudySubject creating was started.");

        var tag = mapper.Map<StudySubject>(dto);

        var newTag = await repository.Create(tag).ConfigureAwait(false);

        logger.LogDebug($"StudySubject with Id = {newTag?.Id} created successfully.");

        return mapper.Map<StudySubjectDto>(newTag);
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Subject with Id = {id} started.");

        var entity = await repository.GetById(id).ConfigureAwait(false);

        if (entity is null)
        {
            logger.LogWarning($"Subject with Id = {id} was not found.");
            throw new KeyNotFoundException($"Subject with Id = {id} does not exist.");
        }

        try
        {
            await repository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation($"Subject with Id = {id} successfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting Subject with Id = {id} failed.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StudySubjectDto>> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation("Getting all Subjects started.");

        var subjects = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!subjects.Any()
            ? "Subject table is empty."
            : $"All {subjects.Count()} records were successfully received from the Subject table");

        return subjects.Select(subject => mapper.Map<StudySubjectDto>(subject)).ToList();
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> GetById(long id, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogInformation($"Getting Subject by Id started. Looking Id = {id}.");

        var subject = await repository.GetById(id).ConfigureAwait(false);

        if (subject == null)
        {
            throw new ArgumentException(
                nameof(id),
                paramName: $"There are no recors in subjects table with such id - {id}.");
        }

        logger.LogInformation($"Got a Subject with Id = {id}.");

        return mapper.Map<StudySubjectDto>(subject);
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> Update(StudySubjectDto dto, LocalizationType localization = LocalizationType.Ua)
    {
        logger.LogDebug($"Updating StudySubject with Id = {dto.Id}, {localization} localization, started.");

        var Localized = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (Localized == null)
        {
            logger.LogError($"Updating failed. Tag with Id = {dto.Id} doesn't exist in the system.");

            return null;
        }

        var tag = await repository.Update(Localized).ConfigureAwait(false);

        logger.LogDebug($"Tag with Id = {tag.Id} updated succesfully.");

        return mapper.Map<StudySubjectDto>(tag);
    }
}
