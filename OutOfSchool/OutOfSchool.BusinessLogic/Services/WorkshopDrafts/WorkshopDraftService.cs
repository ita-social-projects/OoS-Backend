using AutoMapper;
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
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;


/// <summary>
/// Implements the interface with CRUD functionality for WorkshopDraft entity.
/// </summary>
public class WorkshopDraftService : IWorkshopDraftService, ISensitiveWorkshopDraftService
{
    private readonly ILogger<WorkshopDraftService> logger;
    private readonly IWorkshopDraftRepository workshopDraftRepository;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService;
    private readonly IProviderService providerService;
    private readonly ICurrentUserService currentUserService;
    private readonly IEmployeeService employeeService;
    private readonly IWorkshopServicesCombinerV2 workshopServicesCombinerV2;
    private readonly IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService;
    private readonly IEntityRepository<long, Tag> tagRepository;
    private readonly IRegionAdminService regionAdminService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly ISearchStringService searchStringService;
    private readonly IInstitutionHierarchyRepository institutionHierarchyRepository;
    private readonly ICodeficatorRepository codeficatorRepository;
    private readonly int maxParallelUploads;

    /// <summary>
    /// Create a delegate to include other entities in InstitutionHierarchy entity
    /// </summary>
    private readonly Func<IQueryable<InstitutionHierarchy>, IQueryable<InstitutionHierarchy>> includeDirectionsFunc =
        i => i.Include(i => i.SubDirections);

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopDraftService"/> class.
    /// </summary>
    /// <param name="logger">Logger for error logging.</param>
    /// <param name="workshopDraftRepository">Repository for the <see cref="WorkshopDraft"/> entity, handling CRUD operations.</param>
    /// <param name="mapper">Service for mapping between domain models and DTOs.</param>
    /// <param name="workshopDraftImagesService">Service for handling images associated with <see cref="WorkshopDraft"/> entities.</param>
    /// <param name="providerService">Service for handling CRUD operations with the <see cref="Provider"/> entity .</param>
    /// <param name="teacherDraftImagesService">Service for managing cover images for <see cref="TeacherDraft"/> entities.</param>
    /// <param name="tagRepository">Repository for the <see cref="Tag"/> entity, used for CRUD operations.</param>
    /// <param name="options">Provides configuration settings for upload concurrency.</param>    
    /// <param name="workshopServicesCombinerV2">Service for managing workshops.</param>
    /// <param name="employeeService">Service for managing employees.</param>
    /// <param name="currentUserService">Service for managing current user.</param>
    /// <param name="regionAdminService">Service for region admin.</param>
    /// <param name="ministryAdminService"> Service for ministry admin.</param>
    /// <param name="codeficatorService">Service for CATOTTG.</param>
    /// <param name="searchStringService">Service for handling the search string.</param>
    /// <param name="institutionHierarchyRepository">Repository for InstitutionHierarchy.</param>
    /// <param name="codeficatorRepository">Repository for CATOTTG.</param>
    public WorkshopDraftService(
        ILogger<WorkshopDraftService> logger,
        IWorkshopDraftRepository workshopDraftRepository,
        IMapper mapper,
        IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService,
        IProviderService providerService,
        ICurrentUserService currentUserService,
        IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService,
        IEntityRepository<long, Tag> tagRepository,
        IOptions<UploadConcurrencySettings> options,
        IEmployeeService employeeService,
        IWorkshopServicesCombinerV2 workshopServicesCombinerV2,
        IRegionAdminService regionAdminService,
        IMinistryAdminService ministryAdminService,
        ICodeficatorService codeficatorService,
        ISearchStringService searchStringService,
        IInstitutionHierarchyRepository institutionHierarchyRepository,
        ICodeficatorRepository codeficatorRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.workshopDraftRepository = workshopDraftRepository ?? throw new ArgumentNullException(nameof(workshopDraftRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.workshopDraftImagesService = workshopDraftImagesService ?? throw new ArgumentNullException(nameof(workshopDraftImagesService));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.teacherDraftImagesService = teacherDraftImagesService ?? throw new ArgumentNullException(nameof(teacherDraftImagesService));
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        this.maxParallelUploads = options.Value.MaxParallelImageUploads;
        this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        this.workshopServicesCombinerV2 = workshopServicesCombinerV2 ?? throw new ArgumentNullException(nameof(workshopServicesCombinerV2));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
        this.searchStringService = searchStringService ?? throw new ArgumentNullException(nameof(searchStringService));
        this.institutionHierarchyRepository = institutionHierarchyRepository ?? throw new ArgumentNullException(nameof(institutionHierarchyRepository));
        this.codeficatorRepository = codeficatorRepository ?? throw new ArgumentNullException(nameof(codeficatorRepository));
    }

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

        if (!await IsUserProviderOrProviderEmployee(workshopV2Dto.ProviderId))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

        if (workshopV2Dto.Id != Guid.Empty)
        {
            var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopV2Dto.Id, true);

            if (existingWorkshop == null) 
            { 
                workshopV2Dto.Id = Guid.Empty;
            }
            else
            {
                if (!await IsUserProviderOrProviderEmployee(existingWorkshop.ProviderId))
                {
                    throw new UnauthorizedAccessException("User has no rights to perform operation.");
                }
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

            if (!await IsUserProviderOrProviderEmployee(workshopDraft.ProviderId) ||
                !await IsUserProviderOrProviderEmployee(workshopDraftUpdateDto.WorkshopV2Dto.ProviderId))
            {
                throw new UnauthorizedAccessException("User has no rights to perform operation.");
            }

            if (workshopDraftUpdateDto.WorkshopV2Dto.Id != Guid.Empty)
            {
                var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopDraftUpdateDto.WorkshopV2Dto.Id, true);

                if (existingWorkshop == null)
                {
                    workshopDraftUpdateDto.WorkshopV2Dto.Id = Guid.Empty;
                }
                else
                {
                    if (!await IsUserProviderOrProviderEmployee(existingWorkshop.ProviderId))
                    {
                        throw new UnauthorizedAccessException("User has no rights to perform operation.");
                    }
                }
            }

            if (workshopDraft.DraftStatus == WorkshopDraftStatus.PendingModeration)
            {
                throw new ArgumentException("This WorkshopDraft can`t be updated.");
            }

            mapper.Map(workshopDraftUpdateDto.WorkshopV2Dto, workshopDraft);

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
                        Teacher = mapper.Map<TeacherDraftResponseDto>(teacher),
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

        if (!await IsUserProviderOrProviderEmployee(workshopDraft.ProviderId))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

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

        if (!await IsUserProviderOrProviderEmployee(workshopDraft.ProviderId))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

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

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration)
        {
            throw new ArgumentException("This WorkshopDraft can`t be approved.");
        }

        //TODO: Add image loading later

        if (workshopDraft.WorkshopId == null)
        {
            var workshopV2CreateRequestDto = mapper.Map<WorkshopV2CreateRequestDto>(workshopDraft);

            await workshopServicesCombinerV2.Create(workshopV2CreateRequestDto);
        }
        else
        {
            var workshopV2Dto = mapper.Map<WorkshopV2Dto>(workshopDraft);

            await workshopServicesCombinerV2.Update(workshopV2Dto);
        }

        await workshopDraftRepository.Delete(workshopDraft);

        logger.LogDebug("Draft was successfully approved and deleted. Draft Id = {DraftId}.", id);   
    }

    // <inheritdoc/>
    public async Task Reject(Guid id, string rejectionMessage)
    {
        logger.LogDebug("Rejecting WorkshopDraft started. WorkshopDraft Id = {Id}.", id);

        var workshopDraft = await GetWorkshopDraftById(id);

        if (workshopDraft.DraftStatus != WorkshopDraftStatus.PendingModeration)
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

        if (!await IsUserProviderOrProviderEmployee(id))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

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
            var responseDto = mapper.Map<WorkshopDraftViewCardDto>(draft);
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

        IEnumerable<long> allowedSettlementIdsForAdmin = Enumerable.Empty<long>();
        IEnumerable<long> subSettlementsIdsByFilter = Enumerable.Empty<long>();

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

        if (!await IsUserProviderOrProviderEmployee(draft.ProviderId))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

        return await MapWorkshopDraftWithDetails(draft);
    }

    public async Task<WorkshopV2Dto> UpdateWorkshop(WorkshopV2Dto workshopV2Dto)
    {
        logger.LogDebug("Workshop Update started. Workshop Id = {Id}.", workshopV2Dto.Id);

        var existingWorkshop = await workshopServicesCombinerV2.GetById(workshopV2Dto.Id, true);

        if (existingWorkshop == null)
        {
            throw new InvalidOperationException($"There is no Workshop with such Id. Workshop can`t be updated.");
        }

        if (!await IsUserProviderOrProviderEmployee(workshopV2Dto.ProviderId) ||
            !await IsUserProviderOrProviderEmployee(existingWorkshop.ProviderId))
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation.");
        }

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

    private async Task<WorkshopDraft> GetWorkshopDraftById(Guid id)
    {
        logger.LogDebug("Getting WorkshopDraft by Id started. Looking Id = {Id}.", id);

        var workshopDraft = await workshopDraftRepository.GetById(id);

        if (workshopDraft == null)
        {
            throw new ArgumentException(
            nameof(id),
                paramName: $"There are no recors in workshopDrafts table with such id - {id}.");
        }

        logger.LogDebug("Got a WorkshopDraft with Id = {Id}.", id);

        return workshopDraft;
    }

    private async Task<WorkshopDraft> CreateWorkshopDraft(WorkshopV2Dto workshopV2Dto)
    {
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopV2Dto);

        var providerDto = await providerService.GetById(workshopV2Dto.ProviderId);

        workshopDraft.WorkshopDraftContent.ProviderLicenseStatus = providerDto.LicenseStatus;
        workshopDraft.WorkshopDraftContent.OwnershipType = providerDto.Ownership;
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

        Task<MultipleImageUploadingResult> workshopImagesUploadingTasks =
            Task.FromResult<MultipleImageUploadingResult>(null);

        Task<Result<string>> workshopUploadingCoverImageTask = Task.FromResult<Result<string>>(null);

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
                Teacher = mapper.Map<TeacherDraftResponseDto>(teacher),
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
       
    private async Task<bool> IsUserProviderOrProviderEmployee(Guid providerId)
    {
        var userId = currentUserService.UserId;

        var provider = await providerService.GetById(providerId);

        if (provider.UserId == userId)
        {
            return true;
        }

        var employees = await employeeService.GetRelatedEmployees(provider.UserId);
        var employeesIds = employees.Select(emp => emp.Id);

        return employeesIds.Contains(userId);
    }

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) =>
        ModelValidationHelper.ValidateExcludedIdFilter(filter);

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
        var workshopDraftResponseDto = mapper.Map<WorkshopDraftResponseDto>(draft);

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
                address.CodeficatorAddressDto = mapper.Map<AllAddressPartsDto>(
                    catottgs.FirstOrDefault(c => c.Id == address.CATOTTGId))
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
            var responseDto = mapper.Map<WorkshopDraftResponseDto>(draft);

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
                    address.CodeficatorAddressDto = mapper.Map<AllAddressPartsDto>(
                        catottgs.FirstOrDefault(c => c.Id == address.CATOTTGId))
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
}