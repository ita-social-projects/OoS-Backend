using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;
public class StudySubjectService : IStudySubjectService
{
    private readonly IEntityRepositorySoftDeleted<Guid, StudySubject> repository;
    private readonly ILogger<StudySubjectService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public StudySubjectService(
        IEntityRepositorySoftDeleted<Guid, StudySubject> repository,
        ILogger<StudySubjectService> logger,
        IMapper mapper)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<StudySubjectCreateUpdateDto> Create(StudySubjectCreateUpdateDto dto)
    {
        logger.LogInformation("StudySubject creating was started.");

        if (dto is null)
        {
            logger.LogInformation("Creating failed, dto is null.");
            throw new ArgumentException("Dto is null.", nameof(dto));
        }

        var studySubject = mapper.Map<StudySubject>(dto);

        var newStudySubject = await repository.Create(studySubject).ConfigureAwait(false);

        logger.LogDebug($"StudySubject with Id = {newStudySubject?.Id} created successfully.");

        return mapper.Map<StudySubjectCreateUpdateDto>(newStudySubject);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation($"Deleting StudySubject with Id = {id} started.");

        var entity = await repository.GetById(id).ConfigureAwait(false);

        if (entity is null || entity.IsDeleted)
        {
            logger.LogWarning($"StudySubject with Id = {id} was not found.");
            throw new KeyNotFoundException($"StudySubject with Id = {id} does not exist or it was deleted.");
        }

        try
        {
            await repository.Delete(entity).ConfigureAwait(false);
            logger.LogInformation($"StudySubject with Id = {id} successfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting StudySubject with Id = {id} failed.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StudySubjectDto>> GetByFilter(SearchStringFilter filter)
    {
        logger.LogInformation("Getting all StudySubjects by filter started.");

        filter ??= new SearchStringFilter();
        var predicate = PredicateBuilder.True<StudySubject>();

        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            predicate = predicate
                .And(s => s.NameInUkrainian.Contains(filter.SearchString) 
                || s.NameInInstructionLanguage.Contains(filter.SearchString));
        }

        predicate = predicate.And(s => !s.IsDeleted);

        var subjects = await repository
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate
            ).ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!subjects.Any()
            ? "StudySubject table is empty."
            : $"All {subjects.Count()} records were successfully received from the StudySubject table");

        return subjects.Select(mapper.Map<StudySubjectDto>).ToList();
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> GetById(Guid id)
    {
        logger.LogInformation($"Getting StudySubject by Id started. Looking Id = {id}.");

        var subject = await repository.GetById(id).ConfigureAwait(false);

        if (subject == null || subject.IsDeleted)
        {
            throw new ArgumentException(
                nameof(id),
                paramName: $"There are no recors in StudySubjects table with such id - {id}, or such StudySubject was deleted.");
        }

        logger.LogInformation($"Got a StudySubject with Id = {id}.");

        return mapper.Map<StudySubjectDto>(subject);
    }

    /// <inheritdoc/>
    public async Task<StudySubjectCreateUpdateDto> Update(StudySubjectCreateUpdateDto dto)
    {
        logger.LogInformation($"Updating StudySubject started.");

        if (dto is null)
        {
            logger.LogInformation("Updating failed, dto is null.");
            throw new ArgumentException("Dto is null.", nameof(dto));
        }

        try
        {
            var updatedStudySubject = await repository.Update(mapper.Map<StudySubject>(dto))
                .ConfigureAwait(false);
            logger.LogInformation($"StudySubject updated succesfully.");

            return mapper.Map<StudySubjectCreateUpdateDto>(updatedStudySubject);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. StudySubject to update was not found.");
            throw;
        }
    }
}
