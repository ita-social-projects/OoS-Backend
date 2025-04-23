using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.StudySubjects;
using OutOfSchool.Common.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Linq.Expressions;
using static OutOfSchool.BusinessLogic.Util.OperationResultHelper;

namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Initializes a new instance of the <see cref="StudySubjectService"/> class.
/// </summary>
/// <param name="studySubjectRepository">Repository for StudySubject.</param>
/// <param name="workshopRepository">Repository for Workshop.</param>
/// <param name="languageRepository">Repository for Language.</param>
/// <param name="currentUserService">Current User service.</param>
/// <param name="logger">Logger.</param>
public class StudySubjectService(
    IEntityRepositorySoftDeleted<Guid, StudySubject> studySubjectRepository,
    IWorkshopRepository workshopRepository,
    IEntityRepository<long, Language> languageRepository,
    ICurrentUserService currentUserService,
    ILogger<StudySubjectService> logger
) : IStudySubjectService
{
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

        var studySubject = dto.ToModel(providerId);

        await UpdateEntityLanguages(dto, studySubject);

        var newStudySubject = await studySubjectRepository.Create(studySubject).ConfigureAwait(false);

        logger.LogDebug("StudySubject with Id = {Id} created successfully.", newStudySubject?.Id);

        return newStudySubject.ToDto();
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

            return Result<StudySubjectDto>.Success(studySubject.ToDto());
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

        var count = await studySubjectRepository.Count(predicate).ConfigureAwait(false);

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
            Entities = studySubjects.ToDto(),
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

        return studySubject.ToDto();
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
            logger.LogWarning("There are no records in StudySubjects table with such id - {Id}", dto.Id);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"There are no records in StudySubjects table with such id - {dto.Id}",
            });
        }

        await UpdateEntityLanguages(dto, studySubject);

        try
        {
            var updatedStudySubject = await studySubjectRepository.Update(dto.SetToModel(studySubject))
                .ConfigureAwait(false);

            logger.LogDebug("StudySubject updated successfully");

            return Result<StudySubjectDto>.Success(updatedStudySubject.ToDto());
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
                dto.Language = ukrainianLanguage.ToDto();

                logger.LogDebug("Ukrainian language was set in dto as the primary language");
            }
        }
    }

    /// <inheritdoc/>
    public async Task<Result<StudySubjectDto>> UpdateWorkshopsForStudySubject(
        Guid studySubjectId,
        Guid providerId,
        IEnumerable<WorkshopAttachmentStatusDto> workshopsWithStatus)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        // Load the study subject with its workshops
        var studySubject = await studySubjectRepository
            .GetByIdWithDetails(studySubjectId, includeProperties: "Workshops")
            .ConfigureAwait(false);

        if (studySubject == null)
        {
            logger.LogWarning("StudySubject with Id = {StudySubjectId} was not found", studySubjectId);
            return NotFoundResult<StudySubjectDto>(studySubjectId);
        }

        var workshopIds = workshopsWithStatus.Select(w => w.Id).ToList();

        // Get the provider's workshops that match the passed IDs
        var allWorkshops = await workshopRepository
            .GetByFilter(w => workshopIds.Contains(w.Id) && w.ProviderId == providerId)
            .ConfigureAwait(false);

        if (!allWorkshops.Any())
        {
            logger.LogWarning("No workshops found for provider {ProviderId} in study subject {StudySubjectId}",
                              providerId, studySubjectId);
            return NotFoundResult<StudySubjectDto>(providerId);
        }

        // Check if all workshops to attach were found
        var existingWorkshopIds = studySubject.Workshops.Select(w => w.Id).ToList();

        foreach (var workshopDto in workshopsWithStatus)
        {
            var workshop = allWorkshops.FirstOrDefault(w => w.Id == workshopDto.Id);
            if (workshop == null) // Skip if workshop was not found
            {
                logger.LogWarning("Workshop with Id = {WorkshopId} was passed but not found among provider {ProviderId}'s workshops",
                          workshopDto.Id, providerId);
                continue;
            }    

            if (workshopDto.IsAttached)
            {
                // if workshop was attached - detach it
                studySubject.Workshops.RemoveAll(ws => ws.Id == workshop.Id);
            }
            else
            {
                // if workshop was detached - attach it
                if (!existingWorkshopIds.Contains(workshop.Id))
                {
                    studySubject.Workshops.Add(workshop);
                }
            }
        }

        try
        {
            await studySubjectRepository.Update(studySubject).ConfigureAwait(false);
            logger.LogDebug("Updated workshop attachments for StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(studySubject.ToDto());
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
        await currentUserService.UserHasRights(new ProviderRights(providerId)).ConfigureAwait(false);

        // Load the study subject with its workshops
        var studySubject = await studySubjectRepository
            .GetByIdWithDetails(studySubjectId, includeProperties: "Workshops")
            .ConfigureAwait(false);

        if (studySubject == null)
        {
            logger.LogWarning("StudySubject with Id = {StudySubjectId} was not found", studySubjectId);
            return NotFoundResult<StudySubjectDto>(studySubjectId);
        }

        if (!studySubject.Workshops.Any())
        {
            logger.LogInformation("No workshops to detach for StudySubject with Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(null);
        }

        // Extract workshop IDs from the current StudySubject
        var workshopIds = studySubject.Workshops.Select(ws => ws.Id).ToList();

        // Get only the workshops of this provider among the study subject's workshops
        var providerWorkshops = await workshopRepository
            .GetByFilter(w => w.ProviderId == providerId && workshopIds.Contains(w.Id))
            .ConfigureAwait(false);

        if (!providerWorkshops.Any())
        {
            logger.LogInformation("No workshops owned by provider {ProviderId} found for StudySubject {StudySubjectId}", 
                                   providerId, studySubjectId);
            return NotFoundResult<StudySubjectDto>(providerId);
        }

        // Detach only provider's workshops from the study subject
        studySubject.Workshops.RemoveAll(ws => providerWorkshops.Any(pw => pw.Id == ws.Id));

        try
        {
            await studySubjectRepository.Update(studySubject).ConfigureAwait(false);
            logger.LogDebug("Detached all provider-owned workshops from StudySubject with " +
                            "Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Success(null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Failed to detach provider-owned workshops from StudySubject with " +
                                "Id = {StudySubjectId}", studySubjectId);
            return Result<StudySubjectDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Failed to detach provider-owned workshops from StudySubject with Id = {studySubjectId}."
            });
        }
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
