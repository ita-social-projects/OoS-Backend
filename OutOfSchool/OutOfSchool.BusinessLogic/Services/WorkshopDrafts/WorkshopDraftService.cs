using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Collections.Concurrent;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;


/// <summary>
/// Implements the interface with CRUD functionality for WorkshopDraft entity.
/// </summary>
public class WorkshopDraftService : IWorkshopDraftService
{
    private readonly ILogger<WorkshopDraftService> logger;
    private readonly IWorkshopDraftRepository workshopDraftRepository;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService;
    private readonly IProviderService providerService;
    private readonly IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService;
    private readonly IEntityRepository<long, Tag> tagRepository;

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
    public WorkshopDraftService(
        ILogger<WorkshopDraftService> logger,
        IWorkshopDraftRepository workshopDraftRepository,
        IMapper mapper,
        IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService,
        IProviderService providerService,
        IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService,
        IEntityRepository<long, Tag> tagRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.workshopDraftRepository = workshopDraftRepository ?? throw new ArgumentNullException(nameof(workshopDraftRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.workshopDraftImagesService = workshopDraftImagesService ?? throw new ArgumentNullException(nameof(workshopDraftImagesService));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.teacherDraftImagesService = teacherDraftImagesService ?? throw new ArgumentNullException(nameof(teacherDraftImagesService));
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    // <inheritdoc/>
    public async Task<WorkshopDraftResultDto> Create(WorkshopDraftCreateDto workshopDraftDto)
    {
        if (workshopDraftDto == null)
        {
            logger.LogError(
                "ArgumentNullException: While executing the method '{MethodName}'," +
                " the parameter '{ParameterName}' is null.",
                nameof(Create),
                nameof(WorkshopDraftCreateDto));

            throw new ArgumentNullException(nameof(workshopDraftDto));
        }

        logger.LogInformation("Workshop draft creating was started.");

        if (workshopDraftDto.Teachers == null || !workshopDraftDto.Teachers.Any())
        {
            throw new InvalidDataException("The workshop must have at least one associated teacher.");
        }

        // Executes the creation of a workshop draft along with its associated teachers within a database transaction.
        // The result is the created draft with all its related teachers.
        var createdDraftWithAssociatedTeachers = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopDraftDto))
            .ConfigureAwait(false);

        var tags = await tagRepository.GetByFilter(x => createdDraftWithAssociatedTeachers.WorkshopDraftContent.TagsIds.Contains(x.Id))
            .ConfigureAwait(false);

        // Concurrently uploads images for both teacher drafts and the workshop draft.
        var (teacherImagesUploadingResults, workshopImagesUploadingResult, workshopCoverImageUploadingResult) = 
            await UploadWorkshopAndTeacherImagesAsync(createdDraftWithAssociatedTeachers, workshopDraftDto)
            .ConfigureAwait(false);

        await workshopDraftRepository.SaveChangesAsync()
            .ConfigureAwait(false);

        var createdDraftDto = mapper.Map<WorkshopDraftResponseDto>(createdDraftWithAssociatedTeachers);
        createdDraftDto.Tags = mapper.Map<List<TagDto>>(tags);

        logger.LogInformation("WorkshopDraft with Id = {Id} created successfully.", createdDraftWithAssociatedTeachers.Id);

        return new WorkshopDraftResultDto
        {
            WorkshopDraft = createdDraftDto,
            UploadingCoverImgWorkshopResult = workshopCoverImageUploadingResult?.OperationResult,
            UploadingImagesResults = workshopImagesUploadingResult?.MultipleKeyValueOperationResult,
            TeachersCreateUpdateResut = teacherImagesUploadingResults
        };
    }

    private async Task<WorkshopDraft> CreateWorkshopDraft(WorkshopDraftCreateDto workshopDto)
    {
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopDto);

        var providerDto = await providerService.GetById(workshopDto.ProviderId);
        var provider = mapper.Map<Provider>(providerDto);

        workshopDraft.WorkshopDraftContent.ProviderLicenseStatus = provider.LicenseStatus;
        workshopDraft.WorkshopDraftContent.OwnershipType = provider.Ownership;
        workshopDraft.WorkshopDraftContent.WorkshopStatus = WorkshopStatus.Open;

        var createdDraft = await workshopDraftRepository.Create(workshopDraft)
            .ConfigureAwait(false);

        return createdDraft;
    }


    // Applicable if images is stored in the external storage
    private async Task<(
    List<TeacherCreateUpdateResultDto> TeacherImagesUploadingResults,
    MultipleImageUploadingResult WorkshopImagesUploadingResult,
    Result<string> WorkshopCoverImageUploadingResult)>
    UploadWorkshopAndTeacherImagesAsync(
    WorkshopDraft createdDraft,
    WorkshopDraftCreateDto workshopDraftDto)
    {
        var teacherUploadImagesTasks = new List<Task>();
        var teacherUploadImagesResults = new ConcurrentBag<TeacherCreateUpdateResultDto>();
        var semaphore = new SemaphoreSlim(4);

        foreach (var (teacherDto, teacher) in workshopDraftDto.Teachers.Zip(createdDraft.Teachers))
        {
            teacherUploadImagesTasks.Add(UploadTeacherCoverImageAsync(
                    teacherDto,
                    teacher,
                    teacherUploadImagesResults,
                    semaphore));
        }

        Task<MultipleImageUploadingResult> workshopImagesUploadingTasks = 
            Task.FromResult<MultipleImageUploadingResult>(null);

        Task<Result<string>> workshopUploadingCoverImageTask = Task.FromResult<Result<string>>(null);

        if (workshopDraftDto.ImageFiles?.Count > 0)
        {
            createdDraft.Images = new List<Image<WorkshopDraft>>();
            workshopImagesUploadingTasks = workshopDraftImagesService.AddManyImagesAsync(
                createdDraft,
                workshopDraftDto.ImageFiles);
        }

        if (workshopDraftDto.CoverImage != null)
        {
            workshopUploadingCoverImageTask = workshopDraftImagesService.AddCoverImageAsync(
                createdDraft,
                workshopDraftDto.CoverImage);
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

        return (
            teacherUploadImagesResults.ToList(),
            await workshopImagesUploadingTasks.ConfigureAwait(false),
            await workshopUploadingCoverImageTask.ConfigureAwait(false));
    }

    private async Task UploadTeacherCoverImageAsync(
        TeacherDraftCreateDto teacherDto,
        TeacherDraft teacher,
        ConcurrentBag<TeacherCreateUpdateResultDto> teacherResults,
        SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        Result<string> uploadingResult = null;
        try
        {
            if(teacherDto.CoverImage!= null)
            {
                uploadingResult = await teacherDraftImagesService
               .AddCoverImageAsync(teacher, teacherDto.CoverImage);

                if (uploadingResult.Succeeded)
                {
                    teacher.CoverImageId = uploadingResult.Value;
                }
            }
           
            teacherResults.Add(new TeacherCreateUpdateResultDto
            {
                Teacher = mapper.Map<TeacherDraftResponseDto>(teacher),
                UploadingAvatarImageResult = uploadingResult
            });
        }
        finally
        {
            semaphore.Release();
        }
    }
}
