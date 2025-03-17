using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services;
public class StudySubjectService : IStudySubjectService
{
    private readonly IEntityRepositorySoftDeleted<Guid, StudySubject> studySubjectRepository;
    private readonly IEntityRepository<long, Language> languageRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly IWorkshopRepository workshopRepository;
    private readonly ILogger<StudySubjectService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudySubjectService"/> class.
    /// </summary>
    /// <param name="studySubjectRepository">Repository for StudySubject.</param>
    /// <param name="workshopRepository">Repository for Workshop.</param>
    /// <param name="languageRepository">Repository for Language.</param>
    /// <param name="currentUserService">Current User service.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public StudySubjectService(
        IEntityRepositorySoftDeleted<Guid, StudySubject> studySubjectRepository,
        IWorkshopRepository workshopRepository,
        IEntityRepository<long, Language> languageRepository,
        ICurrentUserService currentUserService,
        ILogger<StudySubjectService> logger,
        IMapper mapper)
    {
        this.studySubjectRepository = studySubjectRepository ?? throw new ArgumentNullException(nameof(studySubjectRepository));
        this.languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> Create(StudySubjectCreateUpdateDto dto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);
        
        logger.LogDebug("StudySubject creating was started");

        if (dto is null)
        {
            logger.LogError("Creating failed, dto is null");
            return null;
        }

        await CheckIfLanguageIdIsCorrect(dto);

        var studySubject = mapper.Map<StudySubject>(dto);
        await UpdateEntityLanguages(dto, studySubject);

        var newStudySubject = await studySubjectRepository.Create(studySubject).ConfigureAwait(false);

        logger.LogDebug("StudySubject with Id = {Id} created successfully.", newStudySubject?.Id);

        return mapper.Map<StudySubjectDto>(newStudySubject);
    }

    /// <inheritdoc/>
    public async Task<Result<StudySubjectDto>> Delete(Guid id, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        logger.LogDebug("Deleting StudySubject with Id = {Id} started", id);

        var studySubject = await studySubjectRepository.GetById(id).ConfigureAwait(false);

        if (studySubject is null)
        {
            logger.LogWarning("StudySubject with Id = {Id} was not found", id);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"StudySubject with Id = {id} was not found"
            });
        }

        try
        {
            await studySubjectRepository.Delete(studySubject).ConfigureAwait(false);
            logger.LogDebug("StudySubject with Id = {Id} successfully deleted", id);

            return Result<StudySubjectDto>.Success(mapper.Map<StudySubjectDto>(studySubject));
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting StudySubject with Id = {Id} failed", id);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Deleting StudySubject with Id = {id} failed"
            });
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResult<StudySubjectDto>> GetByFilter(Guid providerId, StudySubjectFilter filter)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        logger.LogDebug("Getting all StudySubjects by filter started");

        filter ??= new StudySubjectFilter();
        var predicate = BuildPredicate(filter);

        int count = await studySubjectRepository.Count(predicate).ConfigureAwait(false);

        var studySubjects = await studySubjectRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate)
            .Include(ss => ss.Language)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the StudySubjects table", studySubjects.Count);

        var result = new SearchResult<StudySubjectDto>
        {
            Entities = mapper.Map<List<StudySubjectDto>>(studySubjects),
            TotalAmount = count
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<StudySubjectDto> GetById(Guid id, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        logger.LogDebug("Getting StudySubject by Id started. Looking Id = {Id}", id);

        var studySubject = await studySubjectRepository.GetById(id)
            .ConfigureAwait(false);

        if (studySubject == null)
        {
            logger.LogError("Getting by id failed, dto is null");
            return null;
        }

        logger.LogDebug("Got a StudySubject with Id = {Id}", id);

        return mapper.Map<StudySubjectDto>(studySubject);
    }

    /// <inheritdoc/>
    public async Task<Result<StudySubjectDto>> Update(StudySubjectCreateUpdateDto dto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        logger.LogDebug("Updating StudySubject started");

        if (dto is null)
        {
            logger.LogError("Updating failed, dto is null");
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Dto is null"
            });
        }

        await CheckIfLanguageIdIsCorrect(dto);

        var studySubject = await studySubjectRepository.GetById(dto.Id).ConfigureAwait(false);

        if (studySubject == null)
        {
            logger.LogWarning("There are no recors in StudySubjects table with such id - {Id}", dto.Id);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"There are no recors in StudySubjects table with such id - {dto.Id}",
            });
        }

        await UpdateEntityLanguages(dto, studySubject);

        mapper.Map(dto, studySubject);

        try
        {
            var updatedStudySubject = await studySubjectRepository.Update(studySubject)
                .ConfigureAwait(false);

            logger.LogDebug("StudySubject updated succesfully");

            return Result<StudySubjectDto>.Success(mapper.Map<StudySubjectDto>(updatedStudySubject));
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Updating failed. StudySubject to update was not found");
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Updating failed. StudySubject to update was not found",
            });
        }
    }

    private static Expression<Func<StudySubject, bool>> BuildPredicate(StudySubjectFilter filter)
    {
        var predicate = PredicateBuilder.True<StudySubject>();

        if (!string.IsNullOrEmpty(filter.SearchString))
        {
            predicate = predicate
                .And(s => s.NameInUkrainian.Contains(filter.SearchString)
                || s.NameInInstructionLanguage.Contains(filter.SearchString));
        }

        if (filter.StartDate.HasValue)
        {
            predicate = predicate.And(s => s.ActiveFrom >= DateOnly.FromDateTime(filter.StartDate.Value.Date));
        }

        if (filter.EndDate.HasValue)
        {
            predicate = predicate.And(s => s.ActiveFrom <= DateOnly.FromDateTime(filter.EndDate.Value.Date));
        }

        predicate = predicate.And(s => !s.IsDeleted);

        return predicate;
    }

    private async Task CheckIfLanguageIdIsCorrect(StudySubjectCreateUpdateDto dto)
    {
        if (dto.IsLanguageUkrainian)
        {
            var query = languageRepository
                .Get(
                    whereExpression: x => x.Code.Equals("uk", StringComparison.OrdinalIgnoreCase)
                ).AsNoTracking();

            var ukrainianLanguage = await query.FirstOrDefaultAsync();

            if (ukrainianLanguage == null)
            {
                logger.LogWarning("Operation failed, Ukrainian language is not found in the database");
                throw new ArgumentException("Ukrainian language is not found in the database.");
            }

            var ukrainianLanguageId = ukrainianLanguage.Id;
            var language = dto.Language;

            if (language == null || ukrainianLanguageId != language.Id)
            {
                dto.Language = mapper.Map<LanguageDto>(ukrainianLanguage);

                logger.LogDebug("Ukrainian language was set in dto as the primary language");
            }
        }
    }

    /// <inheritdoc/>
    public async Task<Result<StudySubjectDto>> UpdateWorkshopsForStudySubject(
        Guid studySubjectId,
        IEnumerable<Guid> workshopIdsToAttach,
        IEnumerable<Guid> workshopIdsToDetach,
        Guid providerId)
    {
        await providerService.HasProviderRights(providerId).ConfigureAwait(false);

        // Load the study subject with its workshops
        var studySubject = await studySubjectRepository
            .GetByIdWithDetails(studySubjectId, includeProperties: "Workshops")
            .ConfigureAwait(false);

        if (studySubject == null)
        {
            return NotFoundResult<StudySubjectDto>(studySubjectId);
        }

        // Load all workshops by given Ids to attach and detach
        var allWorkshops = await workshopRepository
            .GetByIds(workshopIdsToAttach.Concat(workshopIdsToDetach))
            .ConfigureAwait(false);

        var workshopsToAttach = allWorkshops.Where(w => workshopIdsToAttach.Contains(w.Id)).ToList();
        var workshopsToDetach = allWorkshops.Where(w => workshopIdsToDetach.Contains(w.Id)).ToList();

        // Check if all workshops to attach were found
        var existingWorkshopIds = studySubject.Workshops.Select(w => w.Id).ToHashSet();

        foreach (var workshop in workshopsToAttach)
        {
            if (!existingWorkshopIds.Contains(workshop.Id))
            {
                studySubject.Workshops.Add(workshop);
            }
        }

        // Remove workshops that are not in the list of workshops to attach
        studySubject.Workshops.RemoveAll(workshopsToDetach.Contains);

        try
        {
            await studySubjectRepository.Update(studySubject).ConfigureAwait(false);
            logger.LogDebug("Updated workshop attachments for StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(mapper.Map<StudySubjectDto>(studySubject));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Failed to update workshop attachments for StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Failed to update workshop attachments for StudySubject with Id = {studySubjectId}"
            });
        }
    }

    /// <inheritdoc/>
    public async Task<Result<StudySubjectDto>> DetachAllWorkshops(Guid studySubjectId, Guid providerId)
    {
        await providerService.HasProviderRights(providerId).ConfigureAwait(false);

        // Load the study subject with its workshops
        var studySubject = await studySubjectRepository
            .GetByIdWithDetails(studySubjectId, includeProperties: "Workshops")
            .ConfigureAwait(false);

        if (studySubject == null)
        {
            return NotFoundResult<StudySubjectDto>(studySubjectId);
        }

        if (!studySubject.Workshops.Any())
        {
            logger.LogInformation("No workshops to detach for StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(mapper.Map<StudySubjectDto>(studySubject));
        }

        // Clear the workshops collection
        studySubject.Workshops.Clear();

        try
        {
            await studySubjectRepository.Update(studySubject).ConfigureAwait(false);
            logger.LogDebug("Detached all workshops from StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(mapper.Map<StudySubjectDto>(studySubject));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Failed to detach all workshops from StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Failed to detach all workshops from StudySubject with Id = {studySubjectId}"
            });
        }
    }

    /// <summary>
    /// Returns a standardized "Not Found" result for an entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was not found.</typeparam>
    /// <param name="id">The unique identifier of the missing entity.</param>
    /// <returns>A failed <see cref="Result{T}"/> indicating that the entity was not found.</returns>
    private Result<T> NotFoundResult<T>(Guid id)
    {
        var entityName = typeof(T).Name;
        logger.LogWarning("{EntityName} with Id = {Id} was not found", entityName, id);
        return Result<T>.Failed(new OperationError
        {
            Code = "404",
            Description = $"{entityName} with Id = {id} was not found"
        });
    }

    private async Task UpdateEntityLanguages(StudySubjectCreateUpdateDto dto, StudySubject studySubject)
    {
        var languageId = dto.Language.Id;
        var language = await languageRepository.Get(
            whereExpression: l => languageId == l.Id)
            .FirstOrDefaultAsync();

        if (language == null)
        {
            logger.LogWarning("Operation failed, Language with Id = {languageId} was not found.", languageId);
            throw new ArgumentException($"Language with Id = {languageId} was not found");
        }

        studySubject.Language = language;
    }
}
