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

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;

public class WorkshopDraftService : IWorkshopDraftService
{
    private readonly ILogger<WorkshopDraftService> logger;
    private readonly IWorkshopDraftRepository workshopDraftRepository;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService;
    private readonly IProviderService providerService;
    private readonly IEntityCoverImageInteractionService<TeacherDraft> teacherDraftImagesService;
    private readonly IEntityRepository<long, Tag> tagRepository;

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

    /// <inheritdoc/>
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

        var createdDraft = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopDraftDto))
            .ConfigureAwait(false);

        var tags = await tagRepository.GetByFilter(x => createdDraft.WorkshopDraftContent.TagsIds.Contains(x.Id))
            .ConfigureAwait(false);

        var (teacherResults, imagesUploadingResult, coverImageUploadingResult) = await UploadWorkshopAndTeacherImagesAsync(
            createdDraft,
            workshopDraftDto
        ).ConfigureAwait(false);

        await workshopDraftRepository.SaveChangesAsync().ConfigureAwait(false);

        var createdDraftDto = mapper.Map<WorkshopDraftResponseDto>(createdDraft);
        createdDraftDto.Tags = mapper.Map<List<TagDto>>(tags);

        logger.LogInformation("WorkshopDraft with Id = {Id} created successfully.", createdDraft.Id);

        return new WorkshopDraftResultDto
        {
            WorkshopDraft = createdDraftDto,
            UploadingCoverImgWorkshopResult = coverImageUploadingResult?.OperationResult,
            UploadingImagesResults = imagesUploadingResult?.MultipleKeyValueOperationResult,
            TeacherCreateUpdateResut = teacherResults
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
    List<TeacherCreateUpdateResultDto> TeacherResults,
    MultipleImageUploadingResult ImagesUploadingResult,
    Result<string> CoverImageUploadingResult)>
    UploadWorkshopAndTeacherImagesAsync(
        WorkshopDraft createdDraft,
        WorkshopDraftCreateDto workshopDraftDto)
    {
        var semaphore = new SemaphoreSlim(4);
        var teacherResults = new List<TeacherCreateUpdateResultDto>();

        var teacherTasks = workshopDraftDto.Teachers.Zip(createdDraft.Teachers)
            .Where(pair => pair.First.CoverImage != null)
            .Select(async pair =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var teacherDto = pair.First;
                    var teacher = pair.Second;

                    var uploadingResult = await teacherDraftImagesService
                        .AddCoverImageAsync(teacher, teacherDto.CoverImage)
                        .ConfigureAwait(false);

                    if (uploadingResult.Succeeded)
                    {
                        teacher.CoverImageId = uploadingResult.Value;
                    }

                    lock (teacherResults)
                    {
                        teacherResults.Add(new TeacherCreateUpdateResultDto
                        {
                            Teacher = mapper.Map<TeacherDraftResponseDto>(teacher),
                            UploadingAvatarImageResult = uploadingResult
                        });
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

        Task<MultipleImageUploadingResult> imagesUploadingTask = Task.FromResult<MultipleImageUploadingResult>(null);
        if (workshopDraftDto.ImageFiles?.Count > 0)
        {
            createdDraft.Images = new List<Image<WorkshopDraft>>();
            imagesUploadingTask = workshopDraftImagesService.AddManyImagesAsync(createdDraft, workshopDraftDto.ImageFiles);
        }

        Task<Result<string>> uploadingCoverImageTask = Task.FromResult<Result<string>>(null);
        if (workshopDraftDto.CoverImage != null)
        {
            uploadingCoverImageTask = workshopDraftImagesService.AddCoverImageAsync(createdDraft, workshopDraftDto.CoverImage);
        }

        try
        {
            await Task.WhenAll(teacherTasks);
            await Task.WhenAll(imagesUploadingTask, uploadingCoverImageTask);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while uploading images.");
            throw;
        }

        return (
            teacherResults,
            await imagesUploadingTask.ConfigureAwait(false),
            await uploadingCoverImageTask.ConfigureAwait(false)
        );
    }
}
