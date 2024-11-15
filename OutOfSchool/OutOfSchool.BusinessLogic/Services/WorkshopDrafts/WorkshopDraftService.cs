using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;

public class WorkshopDraftService : IWorkshopDraftService
{
    private readonly ILogger<WorkshopDraftService> logger;
    private readonly IWorkshopDraftRepository workshopDraftRepository;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService;

    public WorkshopDraftService(
        ILogger<WorkshopDraftService> logger,
        IWorkshopDraftRepository workshopDraftRepository,
        IMapper mapper,
        IImageDependentEntityImagesInteractionService<WorkshopDraft> workshopDraftImagesService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.workshopDraftRepository = workshopDraftRepository ?? throw new ArgumentNullException(nameof(workshopDraftRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.workshopDraftImagesService = workshopDraftImagesService ?? throw new ArgumentNullException(nameof(workshopDraftImagesService));
    }

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

        var (createdDraft, imagesUploadResult, coverImageUploadResult) = await workshopDraftRepository
            .RunInTransaction(() => CreateWorkshopDraft(workshopDraftDto))
            .ConfigureAwait(false);

        if (createdDraft == null)
        {
            throw new InvalidOperationException("The object returned while creating the workshop draft is not initialized.");
        }

        logger.LogInformation("WorkshopDraft with Id = {Id} created successfully.", createdDraft.Id);

        return new WorkshopDraftResultDto
        {
            WorkshopDraft = mapper.Map<WorkshopDraftResponseDto>(createdDraft),
            UploadingCoverImageResult = coverImageUploadResult?.OperationResult,
            UploadingImagesResults = imagesUploadResult?.MultipleKeyValueOperationResult,
        };
    }

    private async Task<(
        WorkshopDraft CreatedDraft,
        MultipleImageUploadingResult ImagesUploadResult,
        Result<string> CoverImageUploadResult)>
        CreateWorkshopDraft(WorkshopDraftCreateDto workshopDto)
    {
        var workshopDraft = mapper.Map<WorkshopDraft>(workshopDto);
        var createdDraft = await workshopDraftRepository.Create(workshopDraft)
            .ConfigureAwait(false);

        MultipleImageUploadingResult imagesUploadingResult = null;
        if (workshopDto.ImageFiles?.Count > 0)
        {
            createdDraft.Images = new List<Image<WorkshopDraft>>();
            imagesUploadingResult = await workshopDraftImagesService
                .AddManyImagesAsync(createdDraft, workshopDto.ImageFiles)
                .ConfigureAwait(false);
        }

        Result<string> uploadingCoverImageResult = null;
        if (workshopDto.CoverImage != null)
        {
            uploadingCoverImageResult = await workshopDraftImagesService
                .AddCoverImageAsync(createdDraft, workshopDto.CoverImage)
                .ConfigureAwait(false);
        }

        await workshopDraftRepository.SaveChangesAsync().
            ConfigureAwait(false);

        return (createdDraft, imagesUploadingResult, uploadingCoverImageResult);
    }
}
