using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
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
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
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
/// <param name="tagRepository">Repository for the <see cref="Tag"/> entity, used for CRUD operations.</param>
/// <param name="options">Provides configuration settings for upload concurrency.</param>    
/// <param name="workshopServicesCombinerV2">Service for managing workshops.</param>
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
    IWorkshopDraftRepository workshopDraftRepository,
    IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService,
    IProviderService providerService,
    ICurrentUserService currentUserService,
    IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService,
    IEntityRepository<long, Tag> tagRepository,
    IOptions<UploadConcurrencySettings> options,
    IWorkshopServicesCombinerV2 workshopServicesCombinerV2,
    IRegionAdminService regionAdminService,
    IMinistryAdminService ministryAdminService,
    ICodeficatorService codeficatorService,
    ISearchStringService searchStringService,
    IInstitutionHierarchyRepository institutionHierarchyRepository,
    ICodeficatorRepository codeficatorRepository,
    IChangesLogService changesLogService
) : IWorkshopDraftService, ISensitiveWorkshopDraftService
{
    private readonly int maxParallelUploads = options.Value.MaxParallelImageUploads;

    /// <summary>
    /// Create a delegate to include other entities in InstitutionHierarchy entity
    /// </summary>
    private readonly Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>> includeDirectionsFunc =
        i => i.Include(i => i.SubDirections);

    // <inheritdoc/>
    public async Task<WorkshopDraftResultDto> Create(WorkshopV2Dto workshopV2Dto)
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

        if (workshopV2Dto.Id != Guid.Empty)
        {
            var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopV2Dto.Id, true);

            if (existingWorkshop == null) 
            { 
                workshopV2Dto.Id = Guid.Empty;
            }
            else
            {
                await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);
            }
        }       

        // Executes the creation of a workshop draft along with its associated teachers within a database transaction.
        // The result is the created draft with all its related teachers.
        var createdDraftWithAssociatedTeachers = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopV2Dto))
            .ConfigureAwait(false);

        var tags = await tagRepository.GetByFilter(
            x => createdDraftWithAssociatedTeachers.WorkshopDraftContent.TagIds.Contains(x.Id))
            .ConfigureAwait(false);

        // Concurrently uploads images for both teacher drafts and the workshop draft.
        var uploadImagesResult = await UploadWorkshopAndTeacherImagesAsync(
            createdDraftWithAssociatedTeachers,
            workshopV2Dto)
           .ConfigureAwait(false);

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
                    await currentUserService.UserHasRights(new ProviderRights(existingWorkshop.ProviderId), new EmployeeRights(existingWorkshop.ProviderId)).ConfigureAwait(false);
                }
            }

            if (workshopDraft.DraftStatus == WorkshopDraftStatus.PendingModeration)
            {
                throw new ArgumentException("This WorkshopDraft can`t be updated.");
            }

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

        var workshopDraft = await GetWorkshopDraftById(id);

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
    public async Task Approve(Guid id)
    {
        //TODO: Check if we can add RunInTransaction later

        logger.LogDebug("Approving WorkshopDraft started. WorkshopDraft Id = {Id}.", id);
                
        var workshopDraft = await GetWorkshopDraftById(id);

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration &&
            workshopDraft.DraftStatus != WorkshopDraftStatus.EditedByModerator)
        {
            throw new ArgumentException("This WorkshopDraft can`t be approved.");
        }

        //TODO: Add image loading later

        if (workshopDraft.WorkshopId == null)
        {
            await workshopServicesCombinerV2.Create(workshopDraft.ToV2CreateRequestDto());
        }
        else
        {
            await workshopServicesCombinerV2.Update(workshopDraft.ToDto());
        }

        await workshopDraftRepository.Delete(workshopDraft);

        logger.LogDebug("Draft was successfully approved and deleted. Draft Id = {DraftId}.", id);   
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
    public async Task<SearchResult<WorkshopDraftViewCardDto>> GetByProviderId(Guid id, ExcludeIdFilter filter)
    {
        logger.LogDebug("Getting Workshop Draft by organization started. Looking ProviderId = {Id}.", id);

        await currentUserService.UserHasRights(new ProviderRights(id), new EmployeeRights(id)).ConfigureAwait(false);

        filter ??= new ExcludeIdFilter();
        ValidateExcludedIdFilter(filter);

        var workshopBaseCardsCount = await workshopDraftRepository.Count(whereExpression: x =>
            filter.ExcludedId == null
                ? (x.ProviderId == id)
                : (x.ProviderId == id && x.Id != filter.ExcludedId)).ConfigureAwait(false);

        var workshopDrafts = await workshopDraftRepository.Get(
                skip: filter.From,
                take: filter.Size,               
                whereExpression: x => filter.ExcludedId == null
                    ? (x.ProviderId == id)
                    : (x.ProviderId == id && x.Id != filter.ExcludedId)).ToListAsync().ConfigureAwait(false);

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

        var predicate = PredicateBuildForAdminds(
            filter,
            adminInstitutionId,
            allowedSettlementIdsForAdmin,
            subSettlementsIdsByFilter);

        var workshopDrafts = await workshopDraftRepository.Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate)
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
        var draft = await GetWorkshopDraftById(id);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new ProviderRights(draft.ProviderId), new EmployeeRights(draft.ProviderId)).ConfigureAwait(false);
        }        

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

            return (await Create(workshopV2Dto)).WorkshopDraft.WorkshopDetails;            
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
        Guid userId,
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

        await currentUserService.UserHasRights(new ModeratorRights(userId), new TechAdminRights(userId)).ConfigureAwait(false);

        logger.LogDebug("Updating WorkshopDraft as moderator started. DraftId = {Id}.", draftId);

        var workshopDraft = await GetWorkshopDraftById(draftId);

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
    public async Task<Result<WorkshopDraftResponseDto>> DeleteCoverImageAsModeratorAsync(Guid draftId, Guid userId)
    {
        logger.LogDebug("Deleting cover image as moderator started. WorkshopDraft Id = {Id}.", draftId);

        var validation = await ValidateDraftForModerator(draftId, userId);
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
            return Result<WorkshopDraftResponseDto>.Success(workshopDraft.ToResponseDto());
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
    public async Task<Result<WorkshopDraftResponseDto>> DeleteImageAsModeratorAsync(Guid draftId, Guid userId, string imageId)
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

        var validation = await ValidateDraftForModerator(draftId, userId);

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

            return Result<WorkshopDraftResponseDto>.Success(workshopDraft.ToResponseDto());
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
        Guid userId,
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

        var validation = await ValidateDraftForModerator(draftId, userId);

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

            return Result<WorkshopDraftResponseDto>.Success(workshopDraft.ToResponseDto());
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

        logger.LogDebug("Got a WorkshopDraft with Id = {Id}.", id);

        return workshopDraft;
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

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) 
        => ModelValidationHelper.ValidateExcludedIdFilter(filter);

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
        IEnumerable<long> subSettlementFilterIds)
    {
        var predicate = PredicateBuilder.True<WorkshopDraft>();

        predicate = predicate.And(x => x.DraftStatus == filter.WorkshopDraftStatus);

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
            predicate = predicate.And(x => allowedSettlementIdsForAdmin.Contains(x.WorkshopDraftContent.Address.CATOTTGId));
        }

        if (subSettlementFilterIds != null && subSettlementFilterIds.Any())
        {
            predicate = predicate.And(x => subSettlementFilterIds.Contains(x.WorkshopDraftContent.Address.CATOTTGId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var searchTerms = searchStringService.SplitSearchString(filter.SearchString);

            if (searchTerms.Any())
            {
                var tempPredicate = PredicateBuilder.False<WorkshopDraft>();
                foreach (var word in searchTerms)
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.WorkshopDraftContent.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.WorkshopDraftContent.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.WorkshopDraftContent.ProviderTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.WorkshopDraftContent.ProviderTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.WorkshopDraftContent.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase));
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        return predicate;
    }
   
    private async Task<List<long>> GetDirectionIdsForWorkshopDraft(WorkshopDraft workshopDraft)
    {
        var institutionHierarchyId = workshopDraft.WorkshopDraftContent.InstitutionHierarchyId;

        if (institutionHierarchyId == null)
        {
            return null;
        }

        var institutionHierarchyDto = await institutionHierarchyRepository.GetByIdWithDetails(
            id: (Guid) workshopDraft.WorkshopDraftContent.InstitutionHierarchyId,
            includeExpression: includeDirectionsFunc);

        return institutionHierarchyDto.SubDirections.Select(d => d.DirectionId).ToList();
    }

    private async Task<WorkshopDraftResponseDto> MapWorkshopDraftWithDetails(WorkshopDraft draft)
    {
        var workshopDraftResponseDto = draft.ToResponseDto();

        workshopDraftResponseDto.WorkshopDetails.DirectionIds = await GetDirectionIdsForWorkshopDraft(draft);

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
                address.CodeficatorAddressDto = catottgs
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

            responseDto.WorkshopDetails.Contacts
                .Where(c => c?.Address != null)
                .Select(c => c.Address)
                .ToList()
                .ForEach(address =>
                    address.CodeficatorAddressDto = catottgs
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

    /// <summary>
    /// Validates whether the specified moderator or tech admin is allowed to access and modify the given workshop draft.
    /// Checks the user's permissions, the existence of the draft, and whether it is in an editable status.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft to validate.</param>
    /// <param name="userId">The ID of the user performing the operation.</param>
    /// <returns>
    /// A <see cref="Result{WorkshopDraft}"/> containing the draft if validation is successful,
    /// or a failed result with appropriate error code and description.
    /// </returns>
    private async Task<Result<WorkshopDraft>> ValidateDraftForModerator(Guid draftId, Guid userId)
    {
        await currentUserService.UserHasRights(new ModeratorRights(userId), new TechAdminRights(userId)).ConfigureAwait(false);

        var workshopDraft = await GetWorkshopDraftById(draftId);

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
}