using Microsoft.Extensions.Options;
using NuGet.Packaging;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using static OutOfSchool.BusinessLogic.Util.OperationResultHelper;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;

/// <summary>
/// Implements the interface with CRUD functionality for WorkshopDraft entity.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WorkshopDraftService"/> class.
/// </remarks>
/// <param name="logger">Logger for error logging.</param>
/// <param name="workshopDraftRepository">Repository for the <see cref="WorkshopDraft"/> entity, handling CRUD operations.</param>
/// <param name="workshopDraftImagesService">Service for handling images associated with <see cref="WorkshopDraft"/> entities.</param>
/// <param name="providerService">Service for handling CRUD operations with the <see cref="Provider"/> entity .</param>
/// <param name="teacherDraftImagesService">Service for managing cover images for <see cref="TeacherDraft"/> entities.</param>
/// <param name="options">Provides configuration settings for upload concurrency.</param>    
/// <param name="workshopServicesCombinerV2">Service for managing workshops.</param>
/// <param name="languageService"> Service for  language managing</param>
/// <param name="currentUserService">Service for managing current user.</param>
/// <param name="regionAdminService">Service for region admin.</param>
/// <param name="ministryAdminService"> Service for ministry admin.</param>
/// <param name="codeficatorService">Service for CATOTTG.</param>
/// <param name="searchStringService">Service for handling the search string.</param>
/// <param name="institutionHierarchyRepository">Repository for InstitutionHierarchy.</param>
/// <param name="codeficatorRepository">Repository for CATOTTG.</param>
/// <param name="changesLogService">Service for changes log.</param>
public class WorkshopDraftService(
    ILogger<WorkshopDraftService> logger,
    ISportsRegistryProviderService sportsRegistryApiService,
    ILanguageService languageService,
    IWorkshopDraftRepository workshopDraftRepository,
    IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService,
    IProviderService providerService,
    ICurrentUserService currentUserService,
    IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService,
    IOptions<UploadConcurrencySettings> options,
    IWorkshopServicesCombinerV2 workshopServicesCombinerV2,
    IRegionAdminService regionAdminService,
    IMinistryAdminService ministryAdminService,
    ICodeficatorService codeficatorService,
    ISearchStringService searchStringService,
    IInstitutionHierarchyRepository institutionHierarchyRepository,
    ICodeficatorRepository codeficatorRepository,
    IChangesLogService changesLogService,
    IInstitutionHierarchyService institutionHierarchyService,
    IOptions<InstitutionOptions> institutionSettings,
    IOptions<ImageStorageOptions> imageStorageOptions
) : IWorkshopDraftService, ISensitiveWorkshopDraftService
{
    private readonly int maxParallelUploads = options.Value.MaxParallelImageUploads;

    /// <summary>
    /// Create a delegate to include other entities in InstitutionHierarchy entity
    /// </summary>
    private readonly Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>> includeDirectionsFunc =
        i => i.Include(i => i.SubDirections);

    // <inheritdoc/>
    public async Task<WorkshopDraftResultDto> Create(WorkshopV2Dto workshopV2Dto, bool fromWorkshop = false)
    {
        if (workshopV2Dto == null)
        {
            logger.LogError(
                "ArgumentNullException: While executing the method '{MethodName}'," +
                " the parameter '{ParameterName}' is null.",
                nameof(Create),
                nameof(WorkshopV2Dto));

            throw new ArgumentNullException(nameof(workshopV2Dto));
        }

        logger.LogDebug("Workshop draft creating was started.");

        await currentUserService.UserHasRights(new ProviderRights(workshopV2Dto.ProviderId), new EmployeeRights(workshopV2Dto.ProviderId)).ConfigureAwait(false);

        bool isNewDraft = true;

        if (workshopV2Dto.Id != Guid.Empty)
        {
            var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopV2Dto.Id, true);

            if (existingWorkshop == null)
            {
                workshopV2Dto.Id = Guid.Empty;
            }
            else
            {
                if (existingWorkshop.Status == WorkshopStatus.Archived)
                {
                    throw new InvalidOperationException("This Workshop is archived. It can not be updated.");
                }
                await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);

                isNewDraft = false;
            }
        }

        if (isNewDraft) ValidateImagesForNewDraft(workshopV2Dto);

        await SetLanguageNameOrThrow(workshopV2Dto).ConfigureAwait(false);
        await ValidateAndAdjustInstitutionHierarchyAsync(workshopV2Dto).ConfigureAwait(false);
        NormalizeConditionalFields(workshopV2Dto);

        // Executes the creation of a workshop draft along with its associated teachers within a database transaction.
        // The result is the created draft with all its related teachers.
        var createdDraftWithAssociatedTeachers = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopV2Dto))
            .ConfigureAwait(false);

        // Concurrently uploads images for both teacher drafts and the workshop draft.
        var uploadImagesResult = await UploadWorkshopAndTeacherImagesAsync(
            createdDraftWithAssociatedTeachers,
            workshopV2Dto)
           .ConfigureAwait(false);

        if (fromWorkshop)
        {
            createdDraftWithAssociatedTeachers.Images ??= [];
            createdDraftWithAssociatedTeachers.Images.AddRange(
                workshopV2Dto.ImageIds.Select(id => new Image<WorkshopDraft> { ExternalStorageId = id }));
        }

        await workshopDraftRepository.SaveChangesAsync()
            .ConfigureAwait(false);

        logger.LogDebug("WorkshopDraft created successfully.");

        return new WorkshopDraftResultDto
        {
            WorkshopDraft = await MapWorkshopDraftWithDetails(createdDraftWithAssociatedTeachers),
            UploadingCoverImgWorkshopResult = uploadImagesResult.WorkshopCoverImageUploadingResult,
            UploadingImagesResults = uploadImagesResult.WorkshopImagesUploadingResult?.MultipleKeyValueOperationResult,
            TeachersCreateUpdateResult = uploadImagesResult.TeacherImagesUploadingResults
        };
    }

    // <inheritdoc/>
    public async Task<WorkshopDraftResultDto> CreateDraftForReactivation(Guid id)
    {
        logger.LogDebug("Creating draft for reactivation started. Workshop Id = {Id}.", id);

        var existingWorkshop = await workshopServicesCombinerV2.GetById(id, true);

        if (existingWorkshop == null)
        {
            logger.LogError("Getting workshop with Id = {id} failed.", id);
            throw new ArgumentException($"Workshop with Id = {id} not found.");
        }

        if (existingWorkshop.Status != WorkshopStatus.Closed)
        {
            throw new InvalidOperationException("This Workshop is not closed. It can not be reactivated.");
        }

        await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);

        var workshopV2Dto = existingWorkshop.ToModel().ToV2Dto();

        var createdDraftWithAssociatedTeachers = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopV2Dto))
            .ConfigureAwait(false);

        var uploadImagesResult = await UploadWorkshopAndTeacherImagesAsync(
            createdDraftWithAssociatedTeachers,
            workshopV2Dto)
           .ConfigureAwait(false);

        await workshopDraftRepository.SaveChangesAsync()
            .ConfigureAwait(false);

        logger.LogDebug("WorkshopDraft for reactivation created successfully.");

        return new WorkshopDraftResultDto
        {
            WorkshopDraft = await MapWorkshopDraftWithDetails(createdDraftWithAssociatedTeachers),
            UploadingCoverImgWorkshopResult = uploadImagesResult.WorkshopCoverImageUploadingResult,
            UploadingImagesResults = uploadImagesResult.WorkshopImagesUploadingResult?.MultipleKeyValueOperationResult,
            TeachersCreateUpdateResult = uploadImagesResult.TeacherImagesUploadingResults
        };
    }

    // <inheritdoc/>
    public async Task<WorkshopDraftResultDto> Update(WorkshopDraftUpdateDto workshopDraftUpdateDto)
    {
        if (workshopDraftUpdateDto == null ||
            workshopDraftUpdateDto.WorkshopV2Dto == null)
        {
            throw new ArgumentNullException(nameof(workshopDraftUpdateDto));
        }

        logger.LogDebug("Updating WorkshopDraft started. WorkshopDraft Id = {Id}.", workshopDraftUpdateDto.Id);

        async Task<(WorkshopDraft updatedDraft, ImageChangingResult coverImageResult,
            MultipleImageChangingResult imagesResult, List<TeacherCreateUpdateResultDto> teachersResult)> UpdateDraftWithDependencies()
        {
            var workshopDraft = await GetWorkshopDraftById(workshopDraftUpdateDto.Id);

            await currentUserService.UserHasRights(new ProviderRights(workshopDraft.ProviderId), new EmployeeRights(workshopDraft.ProviderId)).ConfigureAwait(false);
            await currentUserService.UserHasRights(new ProviderRights(workshopDraftUpdateDto.WorkshopV2Dto.ProviderId), new EmployeeRights(workshopDraftUpdateDto.WorkshopV2Dto.ProviderId)).ConfigureAwait(false);

            if (workshopDraftUpdateDto.WorkshopV2Dto.Id != Guid.Empty)
            {
                var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopDraftUpdateDto.WorkshopV2Dto.Id, true);

                if (existingWorkshop == null)
                {
                    workshopDraftUpdateDto.WorkshopV2Dto.Id = Guid.Empty;
                }
                else
                {
                    if (existingWorkshop.Status == WorkshopStatus.Archived)
                    {
                        throw new InvalidOperationException("This Workshop is archived. It can not be updated.");
                    }
                    await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);
                }
            }

            if (workshopDraft.DraftStatus == WorkshopDraftStatus.PendingModeration)
            {
                throw new ArgumentException("This WorkshopDraft can`t be updated.");
            }
            await SetLanguageNameOrThrow(workshopDraftUpdateDto.WorkshopV2Dto).ConfigureAwait(false);
            await ValidateAndAdjustInstitutionHierarchyAsync(workshopDraftUpdateDto.WorkshopV2Dto).ConfigureAwait(false);

            workshopDraftUpdateDto.WorkshopV2Dto.SetToDraft(workshopDraft);

            var coverImageResult = await workshopDraftImagesService.ChangeCoverImageAsync(
                workshopDraft,
                workshopDraftUpdateDto.WorkshopV2Dto.CoverImageId,
                workshopDraftUpdateDto.WorkshopV2Dto.CoverImage);

            var imagesResult = await workshopDraftImagesService.ChangeImagesAsync(
                workshopDraft,
                workshopDraftUpdateDto.WorkshopV2Dto.ImageIds,
                workshopDraftUpdateDto.WorkshopV2Dto.ImageFiles);

            var teacherCreateUpdateResult = new List<TeacherCreateUpdateResultDto>();

            if (workshopDraftUpdateDto.WorkshopV2Dto.Teachers != null)
            {
                foreach (var (teacher, teacherDTO) in workshopDraft.Teachers.Zip(workshopDraftUpdateDto.WorkshopV2Dto.Teachers))
                {
                    var teacherImageResult = await teacherDraftImagesService.ChangeCoverImageAsync(
                        teacher,
                        teacherDTO.CoverImageId,
                        teacherDTO.CoverImage);

                    teacherCreateUpdateResult.Add(new TeacherCreateUpdateResultDto()
                    {
                        Teacher = teacher.ToResponseDto(),
                        UploadingCoverImageResult = teacherImageResult?.UploadingResult?.OperationResult
                    });
                }
            }

            await workshopDraftRepository.Update(workshopDraft);
            logger.LogDebug("WorkshopDraft was successfully updated. Draft Id = {DraftId}.", workshopDraftUpdateDto.Id);

            return (workshopDraft, coverImageResult, imagesResult, teacherCreateUpdateResult);
        }

        var (updatedDraft, coverImageResult, imagesResult, teacherCreateUpdateResult) = await workshopDraftRepository
            .RunInTransaction(UpdateDraftWithDependencies).ConfigureAwait(false);

        return new WorkshopDraftResultDto()
        {
            WorkshopDraft = await MapWorkshopDraftWithDetails(updatedDraft),
            UploadingCoverImgWorkshopResult = coverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = imagesResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult,
            TeachersCreateUpdateResult = teacherCreateUpdateResult
        };
    }

    // <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogDebug("Deleting WorkshopDraft started. WorkshopDraft Id = {Id}.", id);

        var workshopDraft = await this.GetWorkshopDraftByIdWithImages(id);

        await currentUserService.UserHasRights(new ProviderRights(workshopDraft.ProviderId), new EmployeeRights(workshopDraft.ProviderId)).ConfigureAwait(false);

        if (workshopDraft.DraftStatus == WorkshopDraftStatus.PendingModeration)
        {
            throw new ArgumentException("This WorkshopDraft can`t be deleted.");
        }

        await workshopDraftRepository.Delete(workshopDraft);
        logger.LogDebug("WorkshopDraft was successfully deleted. Draft Id = {DraftId}.", id);
    }

    // <inheritdoc/>
    public async Task SendForModeration(Guid id)
    {
        logger.LogDebug("Sending WorkshopDraft for moderation started. WorkshopDraft Id = {Id}.", id);

        var workshopDraft = await GetWorkshopDraftById(id);

        await currentUserService.UserHasRights(new ProviderRights(workshopDraft.ProviderId), new EmployeeRights(workshopDraft.ProviderId)).ConfigureAwait(false);

        if (workshopDraft.DraftStatus == WorkshopDraftStatus.PendingModeration)
        {
            throw new ArgumentException("This WorkshopDraft can`t be sent for moderation.");
        }

        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;

        await workshopDraftRepository.Update(workshopDraft);
        logger.LogDebug("Draft was successfully sent for moderation. Draft Id = {DraftId}.", id);
    }

    // <inheritdoc/>
    public async Task<Guid> Approve(Guid id)
    {
        //TODO: Check if we can add RunInTransaction later

        Guid createdWorkshopId;
        logger.LogDebug("Approving WorkshopDraft started. WorkshopDraft Id = {Id}.", id);

        var workshopDraft = await GetByIdWithProviderAndWorkshop(id);

        EnsureDraftIsApprovable(workshopDraft);

        //TODO: Add image loading later

        if (workshopDraft.WorkshopId == null)
        {
            if (IsMinistryOfSport(workshopDraft))
            {
                await SyncSectionWithRegistryAsync(workshopDraft);
            }
            var result = await workshopServicesCombinerV2.Create(workshopDraft.ToV2CreateRequestDto());
            createdWorkshopId = result.Workshop.Id;
        }
        else
        {
            await workshopServicesCombinerV2.Update(workshopDraft.ToDto(), true);
            createdWorkshopId = workshopDraft.WorkshopId.Value;
        }

        await workshopDraftRepository.Delete(workshopDraft);

        logger.LogDebug("Draft was successfully approved and deleted. Draft Id = {DraftId}.", id);
        
        return createdWorkshopId;
    }

    // <inheritdoc/>
    public async Task Reject(Guid id, string rejectionMessage)
    {
        logger.LogDebug("Rejecting WorkshopDraft started. WorkshopDraft Id = {Id}.", id);

        var workshopDraft = await GetWorkshopDraftById(id);

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration &&
            workshopDraft.DraftStatus != WorkshopDraftStatus.EditedByModerator)
        {
            throw new ArgumentException("This WorkshopDraft can`t be rejected.");
        }

        workshopDraft.DraftStatus = WorkshopDraftStatus.Rejected;
        workshopDraft.RejectionMessage = rejectionMessage;

        await workshopDraftRepository.Update(workshopDraft);
        logger.LogDebug("Draft was successfully rejected. Draft Id = {DraftId}.", id);
    }

    // <inheritdoc/>
    public async Task<SearchResult<WorkshopDraftViewCardDto>> GetByProviderId(Guid id, WorkshopDraftFilterTitle filter)
    {
        logger.LogDebug("Getting Workshop Draft by organization started. Looking ProviderId = {Id}.", id);

        await currentUserService.UserHasRights(new ProviderRights(id), new EmployeeRights(id)).ConfigureAwait(false);

        filter ??= new WorkshopDraftFilterTitle();
        ValidateWorkshopDraftTitleFilter(filter);

        var predicate = BuildPredicate(filter, id);

        var workshopBaseCardsCount = await workshopDraftRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        var workshopDrafts = await workshopDraftRepository.Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: new Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>()
                {
                    {wd => wd.CreatedAt, SortDirection.Descending},
                    {wd => wd.ModifiedAt, SortDirection.Descending},
                }).ToListAsync().ConfigureAwait(false);

        var institutionHierarchies = await institutionHierarchyRepository.Get(
                whereExpression: i => workshopDrafts.Select(wd => wd.WorkshopDraftContent.InstitutionHierarchyId).Contains(i.Id))
            .IncludeProperties(includeDirectionsFunc)
            .ToListAsync();

        var workshopDraftResponseDtos = new List<WorkshopDraftViewCardDto>();

        foreach (var draft in workshopDrafts)
        {
            var responseDto = draft.ToCardDto();
            responseDto.DirectionIds = institutionHierarchies
                .FirstOrDefault(i =>
                    i.Id == draft.WorkshopDraftContent.InstitutionHierarchyId)
                ?.SubDirections
                .Select(d => d.DirectionId)
                .ToList();

            workshopDraftResponseDtos.Add(responseDto);
        }

        logger.LogDebug(
            "From Workshop Drafts table for provider {Id} were successfully received {Count} records",
            id,
            workshopDraftResponseDtos.Count);

        return new SearchResult<WorkshopDraftViewCardDto>()
        {
            TotalAmount = workshopBaseCardsCount,
            Entities = workshopDraftResponseDtos,
        };
    }

    // <inheritdoc/>
    public async Task<SearchResult<WorkshopDraftResponseDto>> FetchByFilterForAdmins(WorkshopDraftFilterAdministration filter = null)
    {
        logger.LogDebug("Started retrieving Workshop Drafts by filter for admins.");

        filter ??= new WorkshopDraftFilterAdministration();

        var (adminInstitutionId, catottgIdAdmin) = await GetAdminInstitutionAndCatottgIds();

        var allowedSettlementIdsForAdmin = Enumerable.Empty<long>();
        var subSettlementsIdsByFilter = Enumerable.Empty<long>();

        if (catottgIdAdmin > 0)
        {
            allowedSettlementIdsForAdmin = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(catottgIdAdmin)
                .ConfigureAwait(false);
        }

        if (filter.CATOTTGId > 0)
        {
            subSettlementsIdsByFilter = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId)
                .ConfigureAwait(false);
        }

        var searchTerms = !string.IsNullOrWhiteSpace(filter.SearchString) ? searchStringService.SplitSearchString(filter.SearchString)
               .Where(s => !string.IsNullOrWhiteSpace(s))
               .Distinct()
               .ToArray() : Array.Empty<string>();

        var predicate = PredicateBuildForAdminds(
            filter,
            adminInstitutionId,
            allowedSettlementIdsForAdmin,
            subSettlementsIdsByFilter,
            searchTerms);
        var orderBy = BuildSortOrder(searchTerms);

        var workshopDrafts = await workshopDraftRepository.Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: orderBy)
            .Include(d => d.Provider)
                .ThenInclude(p => p.Positions)
                    .ThenInclude(pos => pos.Officials)
                        .ThenInclude(o => o.Individual)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        var workshopDraftsCount = await workshopDraftRepository
            .Count(predicate)
            .ConfigureAwait(false);

        logger.LogDebug("Retrieved {WorkshopsCount} matching records by filter for admins.", workshopDraftsCount);

        return new SearchResult<WorkshopDraftResponseDto>()
        {
            TotalAmount = workshopDraftsCount,
            Entities = await MapWorkshopDraftsCollectionWithDetails(workshopDrafts),
        };
    }

    // <inheritdoc/>
    public async Task<WorkshopDraftResponseDto> GetWorkshopDraftByIdMapped(Guid id)
    {
        var draft = await GetByIdWithProviderDetails(id);

        await currentUserService.UserHasRights(
            new ProviderRights(draft.ProviderId),
            new EmployeeRights(draft.ProviderId),
            new ModeratorRights(),
            new TechAdminRights()
        ).ConfigureAwait(false);


        return await MapWorkshopDraftWithDetails(draft);
    }

    // <inheritdoc/> 
    public async Task<WorkshopV2Dto> UpdateWorkshop(WorkshopV2Dto workshopV2Dto)
    {
        logger.LogDebug("Workshop Update started. Workshop Id = {Id}.", workshopV2Dto.Id);

        var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopV2Dto.Id, true);

        if (existingWorkshop == null)
        {
            throw new InvalidOperationException($"There is no Workshop with such Id. Workshop can`t be updated.");
        }

        if (existingWorkshop.Status == WorkshopStatus.Archived)
        {
            throw new InvalidOperationException("This Workshop is archived. It can not be updated.");
        }
        await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);
        await currentUserService.UserHasRights(new ProviderRights(workshopV2Dto.ProviderId), new EmployeeRights(workshopV2Dto.ProviderId)).ConfigureAwait(false);

        var draft = await workshopDraftRepository.Get(whereExpression: wd => wd.WorkshopId == workshopV2Dto.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (draft != null)
        {
            logger.LogDebug("WorkshopDraft for this Workshop exists. Workshop can`t be updated. Workshop Id = {Id}.", workshopV2Dto.Id);

            throw new InvalidOperationException("WorkshopDraft for this Workshop exists. Workshop can`t be updated.");
        }

        if (AreModeratedFieldsChanged(workshopV2Dto, existingWorkshop))
        {
            logger.LogDebug("Moderated fields was changed. WorkshopDraft creation initiated. Workshop Id = {Id}.", workshopV2Dto.Id);

            return (await Create(workshopV2Dto, true)).WorkshopDraft.WorkshopDetails;
        }

        logger.LogDebug("Moderated fields was not changed. Workshop update initiated. Workshop Id = {Id}.", workshopV2Dto.Id);

        return (await workshopServicesCombinerV2.Update(workshopV2Dto)).Value.Workshop;
    }

    // <inheritdoc/> 
    public async Task<Guid?> GetWorkshopDraftIdByWorkshopId(Guid workshopId)
    {
        var workshopDraft = await workshopDraftRepository.Get(
            whereExpression: wd => wd.WorkshopId == workshopId).FirstOrDefaultAsync();

        if (workshopDraft == null)
        {
            return null;
        }

        return workshopDraft.Id;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDraftResponseDto>> UpdateDraftAsModeratorAsync(
        Guid draftId,
        ModeratorWorkshopDraftEditDto dto)
    {
        if (dto == null)
        {
            logger.LogError("Parameter '{ParameterName}' is null.", nameof(dto));
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "DTO must not be null."
            });
        }

        await currentUserService.UserHasRights(new ModeratorRights(), new TechAdminRights()).ConfigureAwait(false);

        logger.LogDebug("Updating WorkshopDraft as moderator started. DraftId = {Id}.", draftId);

        var workshopDraft = await GetByIdWithProviderDetails(draftId);

        if (workshopDraft is null)
        {
            logger.LogWarning("WorkshopDraft not found. Id = {Id}.", draftId);
            return NotFoundResult<WorkshopDraftResponseDto>(draftId);
        }

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration &&
            workshopDraft.DraftStatus != WorkshopDraftStatus.EditedByModerator)
        {
            logger.LogWarning("WorkshopDraft with Id = {Id} is not editable in current status: {Status}.", draftId, workshopDraft.DraftStatus);
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "409",
                Description = "WorkshopDraft is not editable in its current status."
            });
        }

        var originalContent = workshopDraft.WorkshopDraftContent.DeepCopyModeratorEditable();

        workshopDraft = dto.ToDraft(workshopDraft);

        workshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;

        changesLogService.LogWorkshopDraftChanges(
            originalContent,
            workshopDraft.WorkshopDraftContent,
            workshopDraft.Id,
            currentUserService.UserId);

        await workshopDraftRepository.Update(workshopDraft).ConfigureAwait(false);

        logger.LogInformation("WorkshopDraft successfully updated. Id = {Id}.", draftId);

        return Result<WorkshopDraftResponseDto>.Success(workshopDraft.ToResponseDto());
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDraftResponseDto>> DeleteCoverImageAsModeratorAsync(Guid draftId)
    {
        logger.LogDebug("Deleting cover image as moderator started. WorkshopDraft Id = {Id}.", draftId);

        var validation = await ValidateDraftForModerator(draftId);
        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<WorkshopDraftResponseDto>();
        }

        var workshopDraft = validation.Value;

        if (workshopDraft.CoverImageId == null)
        {
            logger.LogWarning("WorkshopDraft with Id = {Id} doesn't have a cover image to delete.", draftId);
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "No cover image exists for this workshop draft."
            });
        }

        try
        {
            await workshopDraftImagesService.RemoveCoverImageAsync(workshopDraft);
            workshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;

            changesLogService.AddEntityChangesToDbContext(workshopDraft, currentUserService.UserId);

            await workshopDraftRepository.Update(workshopDraft).ConfigureAwait(false);

            logger.LogInformation("Cover image successfully deleted from WorkshopDraft. Id = {Id}.", draftId);

            var workshopDraftWithDetails = await GetByIdWithProviderDetails(draftId);

            return Result<WorkshopDraftResponseDto>.Success(workshopDraftWithDetails.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting cover image for draft with ID {DraftId}.", draftId);
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while deleting the cover image."
            });
        }
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDraftResponseDto>> DeleteImageAsModeratorAsync(Guid draftId, string imageId)
    {
        logger.LogDebug("Deleting image as moderator started. WorkshopDraft Id = {Id}, Image Id = {ImageId}.", draftId, imageId);

        if (string.IsNullOrEmpty(imageId))
        {
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "Image Id must be provided."
            });
        }

        var validation = await ValidateDraftForModerator(draftId);

        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<WorkshopDraftResponseDto>();
        }

        var workshopDraft = validation.Value;

        imageId = Uri.UnescapeDataString(imageId);

        var image = workshopDraft.Images?.FirstOrDefault(i => i.ExternalStorageId == imageId);
        if (image is null)
        {
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"Image with ID {imageId} not found in this workshop draft."
            });
        }

        var oldImageIds = workshopDraft.Images.Select(x => x.ExternalStorageId).ToList();

        try
        {
            await workshopDraftImagesService.RemoveImageAsync(workshopDraft, imageId);

            workshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;

            var newImageIds = workshopDraft.Images.Select(x => x.ExternalStorageId).ToList();

            changesLogService.LogImageDeletions(
                oldImageIds,
                newImageIds,
                workshopDraft.Id,
                "WorkshopDraft",
                currentUserService.UserId);

            await workshopDraftRepository.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Image successfully deleted from WorkshopDraft. Image Id = {ImageId}, Draft Id = {DraftId}.",
                imageId, draftId);

            var workshopDraftWithDetails = await GetByIdWithProviderDetails(draftId);

            return Result<WorkshopDraftResponseDto>.Success(workshopDraftWithDetails.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting image for draft with ID {DraftId}.", draftId);
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while deleting the image."
            });
        }
    }

    /// <inheritdoc/>
    public async Task<Result<WorkshopDraftResponseDto>> DeleteManyImagesAsModeratorAsync(
        Guid draftId,
        IEnumerable<string> imageIds)
    {
        logger.LogDebug("Deleting multiple images as moderator started. WorkshopDraft Id = {Id}.", draftId);

        if (imageIds == null || !imageIds.Any())
        {
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "400",
                Description = "At least one Image Id must be provided."
            });
        }

        var validation = await ValidateDraftForModerator(draftId);

        if (!validation.Succeeded)
        {
            return validation.ToFailedResult<WorkshopDraftResponseDto>();
        }

        var workshopDraft = validation.Value;

        var decodedImageIds = imageIds.Select(Uri.UnescapeDataString).ToList();

        var imagesToDelete = workshopDraft.Images
            .Where(i => decodedImageIds.Contains(i.ExternalStorageId))
            .ToList();

        if (imagesToDelete.Count == 0)
        {
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "404",
                Description = "None of the specified images were found in this workshop draft."
            });
        }

        var oldImageIds = workshopDraft.Images.Select(x => x.ExternalStorageId).ToList();

        try
        {
            await workshopDraftImagesService.RemoveManyImagesAsync(workshopDraft, decodedImageIds);

            var newImageIds = workshopDraft.Images.Select(x => x.ExternalStorageId).ToList();

            workshopDraft.DraftStatus = WorkshopDraftStatus.EditedByModerator;

            changesLogService.LogImageDeletions(
            oldImageIds,
            newImageIds,
            workshopDraft.Id,
            "WorkshopDraft",
            currentUserService.UserId);

            await workshopDraftRepository.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("{Count} images successfully deleted from WorkshopDraft. Draft Id = {DraftId}.",
                imagesToDelete.Count, draftId);

            var workshopDraftWithDetails = await GetByIdWithProviderDetails(draftId);

            return Result<WorkshopDraftResponseDto>.Success(workshopDraftWithDetails.ToResponseDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting images for draft with ID {DraftId}.", draftId);
            return Result<WorkshopDraftResponseDto>.Failed(new OperationError
            {
                Code = "500",
                Description = "An error occurred while deleting the images."
            });
        }
    }

    private static Expression<Func<WorkshopDraft, bool>> BuildPredicate(WorkshopDraftFilterTitle filter, Guid providerId)
    {
        var predicate = PredicateBuilder.True<WorkshopDraft>();

        predicate = predicate.And(x => x.ProviderId == providerId);

        if (filter.ExcludedId.HasValue)
        {
            predicate = predicate.And(x => x.Id != filter.ExcludedId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            predicate = predicate.And(x => x.WorkshopDraftContent.Title.Contains(filter.SearchText, StringComparison.InvariantCultureIgnoreCase));
        }

        return predicate;
    }

    private async Task<WorkshopDraft> GetWorkshopDraftById(Guid id)
    {
        logger.LogDebug("Getting WorkshopDraft by Id started. Looking Id = {Id}.", id);

        var workshopDraft = await workshopDraftRepository.GetById(id);

        if (workshopDraft == null)
        {
            throw new ArgumentException(
            nameof(id),
                paramName: $"There are no records in workshopDrafts table with such id - {id}.");
        }

        logger.LogDebug("Got a WorkshopDraft with Id = {Id}", id);

        return workshopDraft;
    }

    private async Task<WorkshopDraft> GetWorkshopDraftByIdWithImages(Guid id)
    {
        logger.LogDebug("Getting WorkshopDraft by Id started. Looking Id = {Id}.", id);

        var workshopDraft = await workshopDraftRepository.GetByIdWithDetails(id, includeExpression: query =>
            query.Include(wd => wd.Images));

        if (workshopDraft == null)
        {
            throw new ArgumentException(
                nameof(id),
                paramName: $"There are no records in workshopDrafts table with such id - {id}.");
        }

        logger.LogDebug("Got a WorkshopDraft with Id = {Id}", id);

        return workshopDraft;
    }

    private async Task<WorkshopDraft> GetByIdWithProviderDetails(Guid id)
    {
        logger.LogDebug("Getting WorkshopDraft with admin details by Id started. Looking Id = {Id}.", id);

        var workshopDraft = await workshopDraftRepository.GetByIdWithDetails(
            id,
            includeExpression: q => q
            .Include(p => p.Provider)
            .ThenInclude(p => p.Positions)
            .ThenInclude(pos => pos.Officials)
            .ThenInclude(i => i.Individual));

        if (workshopDraft == null)
        {
            throw new ArgumentException(
            nameof(id),
                paramName: $"There are no records in workshopDrafts table with such id - {id}.");
        }

        logger.LogDebug("Got a WorkshopDraft with admin details with Id = {Id}.", id);

        return workshopDraft;
    }

    private async Task<WorkshopDraft> GetByIdWithProviderAndWorkshop(Guid id)
    {
        logger.LogDebug("Getting WorkshopDraft with provider and workshop details by Id {Id}.", id);

        var draft = await workshopDraftRepository.GetByIdWithDetails(
            id,
            includeExpression: q => q
                .Include(d => d.Images)
                .Include(d => d.Provider)
                .ThenInclude(p => p.Institution));

        if (draft is null)
            throw new ArgumentException($"No draft with id {id}.", nameof(id));

        logger.LogDebug("Got WorkshopDraft {Id}. Provider loaded={HasProv}, Workshop loaded={HasWs}",
            id, draft.Provider != null, draft.Workshop != null);

        return draft;
    }


    private async Task<WorkshopDraft> CreateWorkshopDraft(WorkshopV2Dto workshopV2Dto)
    {
        var workshopDraft = workshopV2Dto.ToDraft();

        var licenseStatusAndOwnership = await providerService.GetLicenseStatusAndOwnershipAsync(workshopV2Dto.ProviderId);
        workshopDraft.WorkshopDraftContent.ProviderLicenseStatus = licenseStatusAndOwnership.Item1;
        workshopDraft.WorkshopDraftContent.OwnershipType = licenseStatusAndOwnership.Item2;
        workshopDraft.WorkshopDraftContent.WorkshopStatus = WorkshopStatus.Open;

        var createdDraft = await workshopDraftRepository.Create(workshopDraft)
            .ConfigureAwait(false);

        return createdDraft;
    }

    // Applicable if images is stored in the external storage
    private async Task<UploadImagesResult> UploadWorkshopAndTeacherImagesAsync(
        WorkshopDraft createdDraft,
        WorkshopV2Dto workshopV2Dto)
    {
        var teacherUploadImagesTasks = new List<Task>();
        var teacherUploadImagesResults = new ConcurrentBag<TeacherCreateUpdateResultDto>();
        var semaphore = new SemaphoreSlim(maxParallelUploads);

        if (workshopV2Dto.Teachers != null)
        {
            foreach (var (teacherDto, teacher) in workshopV2Dto.Teachers.Zip(createdDraft.Teachers))
            {
                teacherUploadImagesTasks.Add(UploadTeacherCoverImageAsync(
                        teacherDto,
                        teacher,
                        teacherUploadImagesResults,
                        semaphore));
            }
        }

        var workshopImagesUploadingTasks = Task.FromResult<MultipleImageUploadingResult>(null);

        var workshopUploadingCoverImageTask = Task.FromResult<Result<string>>(null);

        if (workshopV2Dto.ImageFiles?.Count > 0)
        {
            createdDraft.Images = new List<Image<WorkshopDraft>>();
            workshopImagesUploadingTasks = workshopDraftImagesService.AddManyImagesAsync(
                createdDraft,
                workshopV2Dto.ImageFiles);
        }

        if (workshopV2Dto.CoverImage != null)
        {
            workshopUploadingCoverImageTask = workshopDraftImagesService.AddCoverImageAsync(
                createdDraft,
                workshopV2Dto.CoverImage);
        }

        try
        {
            await Task.WhenAll(teacherUploadImagesTasks);
            await Task.WhenAll(workshopImagesUploadingTasks, workshopUploadingCoverImageTask);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error occurred while uploading images for draft with ID {DraftId}.",
                createdDraft.Id);
            throw;
        }

        return new UploadImagesResult()
        {
            TeacherImagesUploadingResults = teacherUploadImagesResults.ToList(),
            WorkshopCoverImageUploadingResult = GetImagesUploadTaskResult(workshopUploadingCoverImageTask, createdDraft.Id)?.OperationResult,
            WorkshopImagesUploadingResult = GetImagesUploadTaskResult(workshopImagesUploadingTasks, createdDraft.Id)
        };
    }

    private async Task UploadTeacherCoverImageAsync(
        TeacherDTO teacherDto,
        TeacherDraft teacher,
        ConcurrentBag<TeacherCreateUpdateResultDto> teacherResults,
        SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        Result<string> uploadingResult = null;
        try
        {
            if (teacherDto.CoverImage != null)
            {
                uploadingResult = await teacherDraftImagesService
                    .AddCoverImageAsync(teacher, teacherDto.CoverImage);
            }

            teacherResults.Add(new TeacherCreateUpdateResultDto
            {
                Teacher = teacher.ToResponseDto(),
                UploadingCoverImageResult = uploadingResult?.OperationResult
            });
        }
        finally
        {
            semaphore.Release();
        }
    }

    private T GetImagesUploadTaskResult<T>(Task<T> task, Guid draftId) where T : class
    {
        if (task == null)
        {
            return null;
        }

        if (!task.IsCompletedSuccessfully)
        {
            logger.LogError(
                     task.Exception,
                     "Images upload task for workshop draft with ID {DraftId} failed due to an exception.",
                     draftId);
            return null;
        }

        return task.Result;
    }

    private static void ValidateWorkshopDraftTitleFilter(WorkshopDraftFilterTitle filter)
        => ModelValidationHelper.ValidateWorkshopDraftTitleFilter(filter);

    private async Task<(Guid InstitutionId, long CatottgId)> GetAdminInstitutionAndCatottgIds()
    {
        if (currentUserService.IsMinistryAdmin())
        {
            var userId = currentUserService.UserId;
            var ministryAdmin = await ministryAdminService
                .GetByUserId(userId)
                .ConfigureAwait(false);

            return (ministryAdmin.InstitutionId, 0);
        }
        else if (currentUserService.IsRegionAdmin())
        {
            var userId = currentUserService.UserId;
            var regionAdmin = await regionAdminService
                .GetByUserId(userId)
                .ConfigureAwait(false);

            if (regionAdmin == null)
            {
                var errorMsg = $"Region admin with the specified ID: {userId} not found";
                logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            return (regionAdmin.InstitutionId, regionAdmin.CATOTTGId);
        }

        return (Guid.Empty, 0);
    }

    private Expression<Func<WorkshopDraft, bool>> PredicateBuildForAdminds(
        WorkshopDraftFilterAdministration filter,
        Guid adminInstitutionId,
        IEnumerable<long> allowedSettlementIdsForAdmin,
        IEnumerable<long> subSettlementFilterIds,
        string[] searchTerms)
    {
        var predicate = PredicateBuilder.True<WorkshopDraft>();

        predicate = predicate.And(x => filter.WorkshopDraftStatuses.Contains(x.DraftStatus));

        if (adminInstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => EF.Functions.JsonUnquote(x.WorkshopDraftContent.InstitutionId.ToString()) == adminInstitutionId.ToString());
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => EF.Functions.JsonUnquote(x.WorkshopDraftContent.InstitutionId.ToString()) == filter.InstitutionId.ToString());
        }

        if (allowedSettlementIdsForAdmin != null && allowedSettlementIdsForAdmin.Any())
        {
            predicate = predicate.And(x => allowedSettlementIdsForAdmin.Contains(x.CATOTTGId));
        }

        if (subSettlementFilterIds != null && subSettlementFilterIds.Any())
        {
            predicate = predicate.And(x => subSettlementFilterIds.Contains(x.CATOTTGId));
        }

        if (searchTerms.Any())
        {
            var tempPredicate = PredicateBuilder.False<WorkshopDraft>();
            foreach (var word in searchTerms)
            {
                var contains = "%" + word + "%";
                tempPredicate = tempPredicate.Or(
                    x =>
                        EF.Functions.Like(EF.Functions.JsonUnquote(x.WorkshopDraftContent.Title), contains) ||
                        EF.Functions.Like(EF.Functions.JsonUnquote(x.WorkshopDraftContent.ShortTitle), contains) ||
                        EF.Functions.Like(x.Provider.FullTitle, contains) ||
                        EF.Functions.Like(x.Provider.FullTitleEn, contains) ||
                        EF.Functions.Like(x.Provider.Edrpou, contains));
            }

            predicate = predicate.And(tempPredicate);
        }

        return predicate;
    }

    /// <summary>
    /// Sort WorkshopDescriptionItemDto in the draft DTO,
    /// as it's faster than doing the JSON field operations in data base.
    /// </summary>
    /// <param name="workshopDescriptionItems">List to sort</param>
    /// <returns>The same list sorted by SectionName</returns>
    private List<WorkshopDescriptionItemDto> SortWorkshopDescriptionItems(List<WorkshopDescriptionItemDto> workshopDescriptionItems)
    {
        return workshopDescriptionItems?.OrderBy(x => x.SectionName.ToLowerInvariant()).ToList();
    }

    private async Task<List<long>> GetDirectionIdsForWorkshopDraft(WorkshopDraft workshopDraft)
    {
        if (workshopDraft?.WorkshopDraftContent?.InstitutionHierarchyId == null)
        {
            return null;
        }

        var institutionHierarchyDto = await institutionHierarchyRepository.GetByIdWithDetails(
            id: (Guid)workshopDraft.WorkshopDraftContent.InstitutionHierarchyId,
            includeExpression: includeDirectionsFunc);

        return institutionHierarchyDto?.SubDirections?.Select(d => d.DirectionId).ToList();
    }

    private async Task<List<long>> GetSubDirectionIdsForWorkshopDraft(WorkshopDraft workshopDraft)
    {
        var institutionHierarchyId = workshopDraft.WorkshopDraftContent.InstitutionHierarchyId;

        if (institutionHierarchyId == null)
            return null;

        var insistutionHierarchyDto = await institutionHierarchyRepository.GetByIdWithDetails(
            id: (Guid)workshopDraft.WorkshopDraftContent.InstitutionHierarchyId,
            includeExpression: includeDirectionsFunc);

        return insistutionHierarchyDto.SubDirections.Select(sd => sd.Id).ToList();
    }

    private async Task<WorkshopDraftResponseDto> MapWorkshopDraftWithDetails(WorkshopDraft draft)
    {
        var workshopDraftResponseDto = draft.ToResponseDto();

        workshopDraftResponseDto.WorkshopDetails.DirectionIds = await GetDirectionIdsForWorkshopDraft(draft);

        workshopDraftResponseDto.WorkshopDetails.SubDirectionIds = await GetSubDirectionIdsForWorkshopDraft(draft);

        workshopDraftResponseDto.WorkshopDetails.WorkshopDescriptionItems = SortWorkshopDescriptionItems(workshopDraftResponseDto.WorkshopDetails.WorkshopDescriptionItems.ToList());

        var catottgIds = workshopDraftResponseDto.WorkshopDetails.Contacts
            .Where(c => c?.Address != null)
            .Select(c => c.Address.CATOTTGId)
            .Distinct()
            .ToList();

        var catottgs = await codeficatorRepository
            .Get(whereExpression: c => catottgIds.Contains(c.Id))
            .ToListAsync();

        workshopDraftResponseDto.WorkshopDetails.Contacts
            .Where(c => c?.Address != null)
            .Select(c => c.Address)
            .ToList()
            .ForEach(address =>
                address.CodeficatorAddress = catottgs
                    .FirstOrDefault(c => c.Id == address.CATOTTGId)
                    ?.ToAllAddressPartsDto()
            );

        return workshopDraftResponseDto;
    }

    private async Task<List<WorkshopDraftResponseDto>> MapWorkshopDraftsCollectionWithDetails(List<WorkshopDraft> workshopDrafts)
    {
        if (!workshopDrafts.Any())
            return new List<WorkshopDraftResponseDto>();

        var institutionHierarchyIds = workshopDrafts
            .Select(wd => wd.WorkshopDraftContent.InstitutionHierarchyId)
            .Distinct()
            .ToList();

        var institutionHierarchies = await institutionHierarchyRepository.Get(
                whereExpression: i => institutionHierarchyIds.Contains(i.Id))
            .IncludeProperties(includeDirectionsFunc)
            .ToListAsync();

        var catottgIds = workshopDrafts
            .Where(wd => wd.WorkshopDraftContent.Contacts != null)
            .SelectMany(wd => wd.WorkshopDraftContent.Contacts)
            .Where(c => c?.Address != null)
            .Select(c => c.Address.CATOTTGId)
            .Distinct()
            .ToList();

        var catottgs = await codeficatorRepository.Get(
                whereExpression: c => catottgIds.Contains(c.Id))
            .ToListAsync();

        return workshopDrafts.Select(draft =>
        {
            var responseDto = draft.ToResponseDto();

            var institutionHierarchy = institutionHierarchies
                .FirstOrDefault(i => i.Id == draft.WorkshopDraftContent.InstitutionHierarchyId);

            responseDto.WorkshopDetails.DirectionIds = institutionHierarchy?.SubDirections
                .Select(d => d.DirectionId)
                .ToList();

            responseDto.WorkshopDetails.SubDirectionIds = institutionHierarchy?.SubDirections
                .Select(sd => sd.Id)
                .ToList();

            responseDto.WorkshopDetails.WorkshopDescriptionItems = SortWorkshopDescriptionItems(responseDto.WorkshopDetails.WorkshopDescriptionItems.ToList());

            responseDto.WorkshopDetails.Contacts
                .Where(c => c?.Address != null)
                .Select(c => c.Address)
                .ToList()
                .ForEach(address =>
                    address.CodeficatorAddress = catottgs
                        .FirstOrDefault(c => c.Id == address.CATOTTGId)
                        ?.ToAllAddressPartsDto()
                );

            return responseDto;

        }).ToList();
    }

    private static bool AreModeratedFieldsChanged(WorkshopV2Dto workshopV2Dto, WorkshopDto existingWorkshop)
    {
        if (workshopV2Dto.CoverImage != null ||
            workshopV2Dto.ImageFiles != null)
        {
            return true;
        }

        if (!(workshopV2Dto.Keywords ?? [])
                .SequenceEqual(existingWorkshop.Keywords ?? []))
        {
            return true;
        }

        if (!workshopV2Dto.WorkshopDescriptionItems.Select(wdi => wdi.SectionName + wdi.Description)
                .SequenceEqual(existingWorkshop.WorkshopDescriptionItems.Select(wdi => wdi.SectionName + wdi.Description)))
        {
            return true;
        }

        var stringFieldsToCompare = new List<Func<WorkshopDto, string>>
        {
            w => w.CompetitiveSelectionDescription,
            w => w.EnrollmentProcedureDescription,
            w => w.PreferentialTermsOfParticipation,
            w => w.ShortTitle,
            w => w.Title
        };

        return stringFieldsToCompare.Any(field =>
        {
            var newValue = field(workshopV2Dto);
            var oldValue = field(existingWorkshop);

            return newValue != oldValue;
        });
    }
    private async Task SetLanguageNameOrThrow(WorkshopV2Dto dto)
    {
        var language = await languageService.GetById(dto.LanguageOfEducationId).ConfigureAwait(false);
        if (language is null)
        {
            var errorMessage = $"Language with ID = {dto.LanguageOfEducationId} was not found.";
            logger.LogWarning(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
        dto.LanguageOfEducationName = language.Name;
    }

    /// <summary>
    /// Validates and updates the InstitutionHierarchy-related properties in the provided DTO:
    /// - Sets WorkshopType to Section and IsChampionPath to true if institution is "Мінспорт".
    /// </summary>
    private async Task ValidateAndAdjustInstitutionHierarchyAsync(WorkshopV2Dto dto)
    {
        if (dto.InstitutionHierarchyId == null)
        {
            throw new InvalidOperationException("InstitutionHierarchyId cannot be null.");
        }

        var institutionHierarchy = await institutionHierarchyRepository
            .GetById(dto.InstitutionHierarchyId.Value)
            .ConfigureAwait(false);

        if (institutionHierarchy == null)
        {
            throw new InvalidOperationException($"InstitutionHierarchy with ID = {dto.InstitutionHierarchyId} was not found.");
        }

        if (institutionHierarchy.Institution == null)
        {
            throw new InvalidOperationException($"Institution not found for InstitutionHierarchy with ID = {dto.InstitutionHierarchyId}.");
        }

        dto.IsChampionPath = institutionHierarchy.Institution.Id.ToString().Equals(
            institutionSettings.Value.MinistryOfSportId,
            StringComparison.OrdinalIgnoreCase);

        if (dto.IsChampionPath)
        {
            dto.WorkshopType = WorkshopType.Section;
        }
    }
    /// <summary>
    /// Validates whether the specified moderator or tech admin is allowed to access and modify the given workshop draft.
    /// Checks the user's permissions, the existence of the draft, and whether it is in an editable status.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft to validate.</param>
    /// <returns>
    /// A <see cref="Result{WorkshopDraft}"/> containing the draft if validation is successful,
    /// or a failed result with appropriate error code and description.
    /// </returns>
    private async Task<Result<WorkshopDraft>> ValidateDraftForModerator(Guid draftId)
    {
        await currentUserService.UserHasRights(new ModeratorRights(), new TechAdminRights()).ConfigureAwait(false);

        var workshopDraft = await GetByIdWithProviderDetails(draftId);

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration &&
            workshopDraft.DraftStatus != WorkshopDraftStatus.EditedByModerator)
        {
            logger.LogWarning("WorkshopDraft with Id = {Id} is not editable in current status: {Status}.",
                draftId, workshopDraft.DraftStatus);
            return Result<WorkshopDraft>.Failed(new OperationError
            {
                Code = "409",
                Description = "WorkshopDraft is not editable in its current status."
            });
        }

        return Result<WorkshopDraft>.Success(workshopDraft);
    }

    /// <summary>
    /// Sets conditional fields in the DTO to null if their corresponding flags are false.
    /// </summary>
    /// <param name="dto">Workshop dto.</param>
    private static void NormalizeConditionalFields(WorkshopV2Dto dto)
    {
        if (!dto.CompetitiveSelection)
        {
            dto.CompetitiveSelectionDescription = null;
        }

        if (!dto.AreThereBenefits)
        {
            dto.PreferentialTermsOfParticipation = null;
        }
    }

    private static Expression<Func<WorkshopDraft, int>> BuildRelevanceScore(string[] searchTerms)
    {
        if (searchTerms == null || searchTerms.Length == 0)
        {
            return wd => 0;
        }

        var wd = Expression.Parameter(typeof(WorkshopDraft), "wd");

        var content = Expression.Property(wd, nameof(WorkshopDraft.WorkshopDraftContent));
        var title = Expression.Property(content, nameof(WorkshopDraftContent.Title));
        var shortTitle = Expression.Property(content, nameof(WorkshopDraftContent.ShortTitle));

        var provider = Expression.Property(wd, nameof(WorkshopDraft.Provider));
        var providerFullTitle = Expression.Property(provider, nameof(Provider.FullTitle));
        var providerFullTitleEn = Expression.Property(provider, nameof(Provider.FullTitleEn));
        var providerEdrpou = Expression.Property(provider, nameof(Provider.Edrpou));

        var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var likeMethod = typeof(DbFunctionsExtensions).GetMethods()
            .Single(m => m.Name == nameof(DbFunctionsExtensions.Like)
                && m.GetParameters().Length == 3);
        
        var jsonUnquoteMethod = typeof(MySqlJsonDbFunctionsExtensions).GetMethods()
            .Single(m => m.Name == nameof(MySqlJsonDbFunctionsExtensions.JsonUnquote)
                && m.GetParameters().Length == 2);

        Expression JsonUnquote(Expression e)
            => Expression.Call(null, jsonUnquoteMethod, efFunctions, e);    

        Expression AddWeighted(Expression cond, int w)
            => Expression.Condition(cond, Expression.Constant(w), Expression.Constant(0));
        Expression Like(Expression e, string pattern)
            => Expression.Call(null, likeMethod, efFunctions, e, Expression.Constant(pattern));
        Expression ProvNotNull(Expression cond)
            => Expression.AndAlso(Expression.NotEqual(provider, Expression.Constant(null, provider.Type)), cond);

        Expression score = Expression.Constant(0);

        foreach (var t in searchTerms)
        {
            var eq = Expression.Constant(t);
            var starts = t + "%";
            var contains = "%" + t + "%";

            var jt = JsonUnquote(title);
            score = Expression.Add(score, AddWeighted(Expression.Equal(jt, eq), 100));
            score = Expression.Add(score, AddWeighted(Like(jt, starts), 80));
            score = Expression.Add(score, AddWeighted(Like(jt, contains), 50));

            var jst = JsonUnquote(shortTitle);
            score = Expression.Add(score, AddWeighted(Expression.Equal(jst, eq), 60));
            score = Expression.Add(score, AddWeighted(Like(jst, starts), 48));
            score = Expression.Add(score, AddWeighted(Like(jst, contains), 30));

            score = Expression.Add(score, AddWeighted(ProvNotNull(Expression.Equal(providerFullTitle, eq)), 40));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerFullTitle, starts)), 32));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerFullTitle, contains)), 20));

            score = Expression.Add(score, AddWeighted(ProvNotNull(Expression.Equal(providerFullTitleEn, eq)), 40));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerFullTitleEn, starts)), 32));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerFullTitleEn, contains)), 20));

            score = Expression.Add(score, AddWeighted(ProvNotNull(Expression.Equal(providerEdrpou, eq)), 30));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerEdrpou, starts)), 24));
            score = Expression.Add(score, AddWeighted(ProvNotNull(Like(providerEdrpou, contains)), 15));
        }

        return Expression.Lambda<Func<WorkshopDraft, int>>(score, wd);
    }

    private static Expression<Func<T, object>> Box<T>(Expression<Func<T, int>> ex)
    {
        var p = ex.Parameters[0];
        var bodyAsObject = Expression.Convert(ex.Body, typeof(object));
        return Expression.Lambda<Func<T, object>>(bodyAsObject, p);
    }

    private static Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection> BuildSortOrder(string[] searchTerms)
    {
        var orderBy = new Dictionary<Expression<Func<WorkshopDraft, object>>, SortDirection>();
        if (searchTerms.Any())
        {
            var scoreExpression = BuildRelevanceScore(searchTerms);
            var scoreObj = Box(scoreExpression);
            orderBy.Add(scoreObj, SortDirection.Descending);
        }

        orderBy.AddRange(new[]
        {
            new KeyValuePair<Expression<Func<WorkshopDraft, object>>, SortDirection>(wd => wd.CreatedAt, SortDirection.Ascending),
            new KeyValuePair<Expression<Func<WorkshopDraft, object>>, SortDirection>(
                wd => wd.DraftStatus == WorkshopDraftStatus.PendingModeration ? 0
                    : wd.DraftStatus == WorkshopDraftStatus.EditedByModerator ? 1
                    : 2,
                SortDirection.Ascending),
            new KeyValuePair<Expression<Func<WorkshopDraft, object>>, SortDirection>(wd => wd.ModifiedAt, SortDirection.Ascending),
            new KeyValuePair<Expression<Func<WorkshopDraft, object>>, SortDirection>(wd => wd.Id, SortDirection.Ascending)
        });

        return orderBy;
    }
    /// <summary>
    /// Asynchronously retrieves the sport kind dictionary ID code associated with the specified workshop draft.
    /// </summary>
    /// <param name="workshopDraft">The workshop draft containing the necessary data to determine the sport kind dictionary ID code.</param>
    /// <returns>The sport kind dictionary ID code as an integer.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <paramref name="workshopDraft"/> is missing a valid InstitutionHierarchyId, if the corresponding
    /// institution hierarchy cannot be found, or if the SportRegistryIdCode is not set.</exception>
    private async Task<long> GetSectionSportKindDictIdCodeAsync(WorkshopDraft workshopDraft)
    {
        if (workshopDraft.WorkshopDraftContent?.InstitutionHierarchyId is not Guid institutionHierarchyId ||
            institutionHierarchyId == Guid.Empty)
        {
            throw new InvalidOperationException("InstitutionHierarchyId is missing in WorkshopDraftContent.");
        }

        var institutionHierarchyDto = await institutionHierarchyService.GetById(institutionHierarchyId);

        if (institutionHierarchyDto is null)
        {
            throw new InvalidOperationException($"InstitutionHierarchy with Id {institutionHierarchyId} not found.");
        }

        if (institutionHierarchyDto.SportRegistryIdCode is null)
        {
            throw new InvalidOperationException(
                $"SportRegistryIdCode is missing for InstitutionHierarchy with Id {institutionHierarchyId}.");
        }

        return (long)institutionHierarchyDto.SportRegistryIdCode;
    }

    /// <summary>
    /// Synchronizes the workshop draft section with the external Sports Registry.
    /// Builds a request from the draft, validates identifiers, 
    /// sends the request to the registry API, and updates the draft with the created registry ID.
    /// </summary>
    /// <param name="draft">The workshop draft to be synchronized.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the draft contains invalid data or the registry synchronization fails.
    /// </exception>
    private async Task SyncSectionWithRegistryAsync(WorkshopDraft draft)
    {
        // Build a request
        var request = draft.ToSportSectionPostRequest(imageStorageOptions.Value.BaseImageUrl);

        if (!long.TryParse(request.SectionAddressLocalityDictIdCode, out var catottgId))
        {
            throw new InvalidOperationException(
                $"Invalid CATOTTG Id: {request.SectionAddressLocalityDictIdCode}");
        }

        request.SectionAddressLocalityDictIdCode = await codeficatorService.GetCodeById(catottgId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(request.SectionAddressLocalityDictIdCode))
        {
            logger.LogError("Codeficator code not found for CATOTTG Id {CatottgId}.", catottgId);
            throw new InvalidOperationException($"Codeficator code not found for CATOTTG Id {catottgId}.");
        }

        request.SectionSportKindDictIdCode = await GetSectionSportKindDictIdCodeAsync(draft);

        // 2) try to check api 
        var apiCreationResponse = await sportsRegistryApiService.RegisterSectionAsync(request);

        // 3) Processing of a result 
        apiCreationResponse.Match(
            error =>
            {
                var details = error.Content ?? error.Message ?? "Unknown";
                logger.LogError("Failed to sync section to Sports Registry. Code={Code}, Message={Message}",
                    (int)error.HttpStatusCode, details);

                // NOTE: Maybe create a specific type of exception in future (TODO) 
                throw new InvalidOperationException($"Registry sync failed: {details}");
            },
            success =>
            {
                var createdRegistryId = success.ResultVariables.SectionId;
                draft.WorkshopDraftContent.MinsportSectionId = createdRegistryId;
                logger.LogInformation(
                    "Workshop draft was successfully synced to Sports Registry. DraftId={DraftId}, CreatedSectionId={SectionId}",
                    draft.Id,
                    createdRegistryId);
                return true;
            }
        );
    }

    /// <summary>
    /// Ensures that the workshop draft can be approved.
    /// Throws an exception if the draft has an invalid status for approval.
    /// </summary>
    /// <param name="draft">The workshop draft to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the draft cannot be approved.</exception>
    private static void EnsureDraftIsApprovable(WorkshopDraft draft)
    {
        var status = draft?.DraftStatus;
        if (status is null ||
            (status != WorkshopDraftStatus.PendingModeration &&
             status != WorkshopDraftStatus.EditedByModerator))
        {
            throw new ArgumentException("This WorkshopDraft can’t be approved.");
        }
    }
    /// <summary>
    /// Determines whether the draft belongs to the Ministry of Sport.
    /// </summary>
    /// <param name="draft">The workshop draft.</param>
    /// <returns><c>true</c> if the provider is associated with the Ministry of Sport; otherwise, <c>false</c>.</returns>
    private bool IsMinistryOfSport(WorkshopDraft draft)
    {
        var institutionId = draft?.Provider?.Institution?.Id.ToString();
        var expected = institutionSettings.Value.MinistryOfSportId;
        return string.Equals(institutionId, expected, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates images for a new draft.
    /// </summary>
    /// <param name="dto">Dto.</param>
    /// <exception cref="ValidationException">Throws validation exception that will be handled in middleware.</exception>
    private static void ValidateImagesForNewDraft(WorkshopV2Dto dto)
    {
        var errors = new StringBuilder();

        bool hasCoverImage = dto.CoverImage is { Length: > 0 };
        bool hasCoverImageId = !string.IsNullOrWhiteSpace(dto.CoverImageId);
        bool hasImageFiles = dto.ImageFiles?.Any(f => f is { Length: > 0 }) ?? false;
        bool hasImageIds = dto.ImageIds?.Any(id => !string.IsNullOrWhiteSpace(id)) ?? false;

        if (hasCoverImageId || hasImageIds)
            errors.Append("For a new draft you must upload image files, not IDs. ");
        if (!hasCoverImage || !hasImageFiles)
            errors.Append("Provide both CoverImage and ImageFiles.");

        if (errors.Length > 0)
            throw new ValidationException(errors.ToString());
    }
}