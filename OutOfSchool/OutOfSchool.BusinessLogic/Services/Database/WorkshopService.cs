using System.Linq.Expressions;
using AutoMapper;
using H3Lib;
using H3Lib.Extensions;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

using SendGrid.Helpers.Errors.Model;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Workshop entity.
/// </summary>
public class WorkshopService : IWorkshopService, ISensitiveWorkshopsService
{
    private readonly string includingPropertiesForMappingDtoModel =
        $"{nameof(Workshop.Address)},{nameof(Workshop.Teachers)},{nameof(Workshop.DateTimeRanges)},{nameof(Workshop.InstitutionHierarchy)}";

    private readonly string includingPropertiesForMappingWorkShopCard = $"{nameof(Workshop.Address)}";

    private readonly IWorkshopRepository workshopRepository;
    private readonly IEntityRepository<long, Tag> tagRepository;
    private readonly IEntityRepositorySoftDeleted<long, DateTimeRange> dateTimeRangeRepository;
    private readonly IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop> roomRepository;
    private readonly ITeacherService teacherService;
    private readonly ILogger<WorkshopService> logger;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<Workshop> workshopImagesService;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IAverageRatingService averageRatingService;
    private readonly IProviderRepository providerRepository;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly ISearchStringService searchStringService;
    private readonly ITagService tagService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopService"/> class.
    /// </summary>
    /// <param name="workshopRepository">Repository for Workshop entity.</param>
    /// <param name="tagRepository">Repository for Tag entity.</param>
    /// <param name="dateTimeRangeRepository">Repository for DateTimeRange entity.</param>
    /// <param name="roomRepository">Repository for ChatRoomWorkshop entity.</param>
    /// <param name="teacherService">Teacher service.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="workshopImagesService">Workshop images mediator.</param>
    /// <param name="employeeRepository">Repository for employees.</param>
    /// <param name="averageRatingService">Average rating service.</param>
    /// <param name="providerRepository">Repository for providers.</param>
    /// <param name="currentUserService">Service that checks the roles and rights current user.</param>
    /// <param name="ministryAdminService"> Service for ministry admin.</param>
    /// <param name="regionAdminService">Service for region admin.</param>
    /// <param name="codeficatorService">Srvice for CATOTTG.</param>
    /// <param name="searchStringService">Service for handling the search string.</param>
    /// <param name="codeficatorService">Service for CATOTTG.</param>
    /// <param name="tagService">Service for Tag entity.</param>
    public WorkshopService(
        IWorkshopRepository workshopRepository,
        IEntityRepository<long, Tag> tagRepository,
        IEntityRepositorySoftDeleted<long, DateTimeRange> dateTimeRangeRepository,
        IEntityRepositorySoftDeleted<Guid, ChatRoomWorkshop> roomRepository,
        ITeacherService teacherService,
        ILogger<WorkshopService> logger,
        IMapper mapper,
        IImageDependentEntityImagesInteractionService<Workshop> workshopImagesService,
        IEmployeeRepository employeeRepository,
        IAverageRatingService averageRatingService,
        IProviderRepository providerRepository,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        ICodeficatorService codeficatorService,
        ITagService tagService,
        ISearchStringService searchStringService)
    {
        this.workshopRepository = workshopRepository;
        this.tagRepository = tagRepository;
        this.dateTimeRangeRepository = dateTimeRangeRepository;
        this.roomRepository = roomRepository;
        this.teacherService = teacherService;
        this.logger = logger;
        this.mapper = mapper;
        this.workshopImagesService = workshopImagesService;
        this.employeeRepository = employeeRepository;
        this.averageRatingService = averageRatingService;
        this.providerRepository = providerRepository;
        this.currentUserService = currentUserService;
        this.ministryAdminService = ministryAdminService;
        this.regionAdminService = regionAdminService;
        this.codeficatorService = codeficatorService;
        this.searchStringService = searchStringService;
        this.tagService = tagService;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopCreateUpdateDto"/> is null.</exception>
    public async Task<WorkshopDto> Create(WorkshopCreateRequestDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation("Workshop creating was started.");

        // TODO: after refactoring the DTOs for the Workshop entities, this method needs to be replaced with the correct mapping
        await SetIdsToDefaultValue(dto); // This method sets the dto properties with Id to the default value.
        var createdWorkshop = await CheckDtoAndPrepareCreatedWorkshop(dto);

        Func<Task<Workshop>> operation = async () =>
            await workshopRepository.Create(createdWorkshop).ConfigureAwait(false);

        var newWorkshop = await workshopRepository.RunInTransaction(operation).ConfigureAwait(false);

        logger.LogInformation("Workshop with Id = {newWorkshopId} created successfully.", newWorkshop.Id);

        return mapper.Map<WorkshopDto>(newWorkshop);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopDto"/> is null.</exception>
    /// <exception cref="InvalidOperationException">If unreal to map teachers.</exception>
    /// <exception cref="DbUpdateException">If unreal to update entity.</exception>
    public async Task<WorkshopResultDto> CreateV2(WorkshopV2CreateRequestDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation("Workshop creating was started.");

        // TODO: after refactoring the DTOs for the Workshop entities, this method needs to be replaced with the correct mapping
        await SetIdsToDefaultValue(dto); // This method sets the properties with the Id to the default value.
        var createdWorkshop = await CheckDtoAndPrepareCreatedWorkshop(dto);

        async Task<(Workshop createdWorkshop, MultipleImageUploadingResult imagesUploadResult, Result<string>
            coverImageUploadResult)> CreateWorkshopAndDependencies()
        {
            var workshop = await workshopRepository.Create(createdWorkshop).ConfigureAwait(false);

            MultipleImageUploadingResult imagesUploadingResult = null;

            if (dto.ImageFiles?.Count > 0)
            {
                workshop.Images = new List<Image<Workshop>>();
                imagesUploadingResult = await workshopImagesService.AddManyImagesAsync(workshop, dto.ImageFiles)
                    .ConfigureAwait(false);
            }

            Result<string> uploadingCoverImageResult = null;
            if (dto.CoverImage != null)
            {
                uploadingCoverImageResult = await workshopImagesService.AddCoverImageAsync(workshop, dto.CoverImage)
                    .ConfigureAwait(false);
            }

            await UpdateWorkshop().ConfigureAwait(false);

            return (workshop, imagesUploadingResult, uploadingCoverImageResult);
        }

        var (newWorkshop, imagesUploadResult, coverImageUploadResult) = await workshopRepository
            .RunInTransaction(CreateWorkshopAndDependencies).ConfigureAwait(false);

        logger.LogInformation($"Workshop with Id = {newWorkshop.Id} created successfully.");

        return new WorkshopResultDto
        {
            Workshop = mapper.Map<WorkshopV2Dto>(newWorkshop),
            UploadingCoverImageResult = coverImageUploadResult?.OperationResult,
            UploadingImagesResults = imagesUploadResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <inheritdoc/>
    public Task<bool> Exists(Guid id)
    {
        logger.LogInformation($"Checking if Workshop exists by Id started. Looking Id = {id}.");

        return workshopRepository.Any(x => x.Id == id);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopDto>> GetAll(OffsetFilter offsetFilter)
    {
        logger.LogInformation("Getting all Workshops started.");

        offsetFilter ??= new OffsetFilter();

        var sortExpression = new Dictionary<Expression<Func<Workshop, object>>, SortDirection>
        {
            { x => x.Id, SortDirection.Ascending },
        };

        var count = await workshopRepository.Count().ConfigureAwait(false);
        var workshops =
            workshopRepository.Get(
                    skip: offsetFilter.From,
                    take: offsetFilter.Size,
                    includeProperties: includingPropertiesForMappingDtoModel,
                    orderBy: sortExpression)
                .ToList();

        logger.LogInformation(!workshops.Any()
            ? "Workshop table is empty."
            : $"All {workshops.Count} records were successfully received from the Workshop table");

        var dtos = mapper.Map<List<WorkshopDto>>(workshops);
        var workshopsWithRating = await GetWorkshopsWithAverageRating(dtos).ConfigureAwait(false);
        return new SearchResult<WorkshopDto>() { TotalAmount = count, Entities = workshopsWithRating };
    }

    /// <inheritdoc/>
    public async Task<WorkshopDto> GetById(Guid id, bool asNoTracking = false)
    {
        logger.LogInformation($"Getting Workshop by Id started. Looking Id = {id}.");

        var workshop = await workshopRepository.GetWithNavigations(id, asNoTracking).ConfigureAwait(false);

        if (workshop == null)
        {
            return null;
        }

        logger.LogInformation($"Successfully got a Workshop with Id = {id}.");

        var workshopDTO = mapper.Map<WorkshopDto>(workshop);

        var rating = await averageRatingService.GetByEntityIdAsync(workshopDTO.Id).ConfigureAwait(false);

        workshopDTO.Rating = rating?.Rate ?? default;
        workshopDTO.NumberOfRatings = rating?.RateQuantity ?? default;

        return workshopDTO;
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetWorkshopListByProviderId(Guid providerId)
    {
        logger.LogDebug("Getting Workshop (Id, Title) by organization started. Looking ProviderId = {ProviderId}",
            providerId);

        var workshops = await workshopRepository.GetByFilter(
            whereExpression: x => x.ProviderId == providerId);

        var result = mapper.Map<List<ShortEntityDto>>(workshops).OrderBy(entity => entity.Title).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetWorkshopListByEmployeeId(string employeeId)
    {
        logger.LogDebug(
            "Getting Workshop (Id, Title) by organization started. Looking EmployeeId = {employeeId}",
            employeeId);

        var employee = (await employeeRepository.GetByFilter(pa => pa.UserId == employeeId)).FirstOrDefault();
        return (await workshopRepository
                .GetByFilter(w => employee.Provider.Workshops.Contains(w)))
            .Select(workshop => mapper.Map<ShortEntityDto>(workshop))
            .OrderBy(workshop => workshop.Title)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopProviderViewCard>> GetByProviderId(Guid id, ExcludeIdFilter filter)
    {
        logger.LogInformation($"Getting Workshop by organization started. Looking ProviderId = {id}.");

        filter ??= new ExcludeIdFilter();
        ValidateExcludedIdFilter(filter);

        var workshopBaseCardsCount = await workshopRepository.Count(whereExpression: x =>
            filter.ExcludedId == null
                ? (x.ProviderId == id)
                : (x.ProviderId == id && x.Id != filter.ExcludedId)).ConfigureAwait(false);

        var workshops = await workshopRepository.Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: includingPropertiesForMappingDtoModel,
                whereExpression: x => filter.ExcludedId == null
                    ? (x.ProviderId == id)
                    : (x.ProviderId == id && x.Id != filter.ExcludedId)).ToListAsync().ConfigureAwait(false);

        var chatrooms = roomRepository.Get(
            skip: 0,
            take: 0,
            includeProperties: "ChatMessages");

        var workshopProviderViewCards = mapper.Map<List<WorkshopProviderViewCard>>(workshops);

        var workshopsIds = workshops.Select(x => x.Id).ToList();

        var query = chatrooms
            .SelectMany(room => room.ChatMessages, (room, message) => new { room, message })
            .Where(entry => entry.message.ReadDateTime == null
                        && !entry.message.SenderRoleIsProvider
                        && workshopsIds.Contains(entry.room.WorkshopId))
            .GroupBy(entry => entry.room.WorkshopId)
            .Select(group => new
            {
                WorkshopId = group.Key,
                UnreadMessageCount = group.Count(),
            });

        var unreadMessages = await query.ToListAsync().ConfigureAwait(false);

        workshopProviderViewCards.ForEach(workshop =>
        {
            var matchingItem = unreadMessages.FirstOrDefault(item => item.WorkshopId == workshop.WorkshopId);
            workshop.UnreadMessages = matchingItem?.UnreadMessageCount ?? 0;
        });

        logger.LogInformation(!workshopProviderViewCards.Any()
            ? $"There aren't Workshops for Provider with Id = {id}."
            : $"From Workshop table were successfully received {workshopProviderViewCards.Count()} records.");

        var result = new SearchResult<WorkshopProviderViewCard>()
        {
            TotalAmount = workshopBaseCardsCount,
            Entities = await GetWorkshopsWithAverageRating(workshopProviderViewCards.ToList()).ConfigureAwait(false),
        };

        return result;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopCreateUpdateDto"/> is null.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<WorkshopDto> Update(WorkshopCreateUpdateDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating Workshop with Id = {dto?.Id} started.");

        async Task<Workshop> UpdateWorkshopLocally()
        {
            await UpdateDateTimeRanges(dto.DateTimeRanges, dto.Id).ConfigureAwait(false);

            var currentWorkshop = await workshopRepository.GetWithNavigations(dto!.Id).ConfigureAwait(false);

            dto.AddressId = currentWorkshop.AddressId;
            dto.Address.Id = currentWorkshop.AddressId;

            await ChangeTeachers(currentWorkshop, dto.Teachers ?? new List<TeacherDTO>()).ConfigureAwait(false);

            if (!dto.TagIds.IsNullOrEmpty())
            {
                var tags = new List<TagDto>();
                foreach (var tagId in dto.TagIds)
                {
                    var tag = await tagService.GetById(tagId);
                    if (tag != null)
                    {
                        var tagDto = mapper.Map<TagDto>(tag);
                        tags.Add(tagDto);
                    }
                }

                currentWorkshop.Tags.Clear();
                currentWorkshop.Tags.AddRange(tags.Select(tagDto => new Tag { Id = tagDto.Id }));
            }

            dto.AvailableSeats = dto.AvailableSeats.GetMaxValueIfNullOrZero();

            await UpdateWorkshopStatusBySeatsLimitAndAvailability(
                (uint)dto.AvailableSeats, currentWorkshop).ConfigureAwait(false);

            mapper.Map(dto, currentWorkshop);

            await UpdateWorkshop().ConfigureAwait(false);

            return currentWorkshop;
        }

        var updatedWorkshop = await workshopRepository
            .RunInTransaction(UpdateWorkshopLocally).ConfigureAwait(false);

        return mapper.Map<WorkshopDto>(updatedWorkshop);
    }

    /// <inheritdoc/>
    public async Task<WorkshopDto> UpdateTags(WorkshopTagsUpdateDto dto)
    {
        logger.LogInformation($"Updating the tags for Workshop with Id = {dto.WorkshopId} started.");

        var workshop = await workshopRepository.GetById(dto.WorkshopId);
        if (workshop == null)
        {
            throw new NotFoundException($"Workshop with Id {dto.WorkshopId} not found.");
        }

        var tags = new List<TagDto>();
        foreach (var tagId in dto.TagIds)
        {
            var tag = await tagService.GetById(tagId);
            if (tag != null)
            {
                var tagDto = mapper.Map<TagDto>(tag);
                tags.Add(tagDto);
            }
        }

        workshop.Tags.Clear();
        workshop.Tags.AddRange(tags.Select(tagDto => new Tag { Id = tagDto.Id }));

        await workshopRepository.Update(workshop);

        return mapper.Map<WorkshopDto>(workshop);
    }

    /// <inheritdoc/>
    public async Task<WorkshopStatusWithTitleDto> UpdateStatus(WorkshopStatusDto dto)
    {
        logger.LogInformation($"Updating Workshop status with Id = {dto.WorkshopId} started.");

        var currentWorkshop = await workshopRepository.GetById(dto.WorkshopId).ConfigureAwait(false);

        if (currentWorkshop is null)
        {
            logger.LogInformation($"Workshop(id) {dto.WorkshopId} not found.");

            return null;
        }

        if (currentWorkshop.Status != dto.Status)
        {
            currentWorkshop.Status = dto.Status;
            try
            {
                await workshopRepository.Update(currentWorkshop).ConfigureAwait(false);
                logger.LogInformation($"Workshop(id) {dto.WorkshopId} Status was changed to {dto.Status}");
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Updating failed. Exception: {exception.Message}");
                throw;
            }
        }

        var dtoWithTitle = mapper.Map<WorkshopStatusWithTitleDto>(dto);
        dtoWithTitle.Title = currentWorkshop.Title;

        return dtoWithTitle;
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<WorkshopResultDto> UpdateV2(WorkshopV2Dto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating {nameof(Workshop)} with Id = {dto.Id} started.");

        async Task<(Workshop updatedWorkshop, MultipleImageChangingResult multipleImageChangingResult,
            ImageChangingResult changingCoverImageResult)> UpdateWorkshopWithDependencies()
        {
            await UpdateDateTimeRanges(dto.DateTimeRanges, dto.Id).ConfigureAwait(false);
            var currentWorkshop = await workshopRepository.GetWithNavigations(dto.Id).ConfigureAwait(false);

            dto.ImageIds ??= new List<string>();
            var multipleImageChangingResult = await workshopImagesService
                .ChangeImagesAsync(currentWorkshop, dto.ImageIds, dto.ImageFiles)
                .ConfigureAwait(false);

            // In case if AddressId was changed. AddressId is one and unique for workshop.
            dto.AddressId = currentWorkshop.AddressId;
            dto.Address.Id = currentWorkshop.AddressId;

            await ChangeTeachers(currentWorkshop, dto.Teachers ?? new List<TeacherDTO>()).ConfigureAwait(false);

            dto.AvailableSeats = dto.AvailableSeats.GetMaxValueIfNullOrZero();

            await UpdateWorkshopStatusBySeatsLimitAndAvailability(
                (uint)dto.AvailableSeats, currentWorkshop).ConfigureAwait(false);

            mapper.Map(dto, currentWorkshop);

            var changingCoverImageResult = await workshopImagesService
                .ChangeCoverImageAsync(currentWorkshop, dto.CoverImageId, dto.CoverImage).ConfigureAwait(false);

            await UpdateWorkshop().ConfigureAwait(false);

            return (currentWorkshop, multipleImageChangingResult, changingCoverImageResult);
        }

        var (updatedWorkshop, multipleImageChangeResult, changeCoverImageResult) = await workshopRepository
            .RunInTransaction(UpdateWorkshopWithDependencies).ConfigureAwait(false);

        return new WorkshopResultDto
        {
            Workshop = mapper.Map<WorkshopV2Dto>(updatedWorkshop),
            UploadingCoverImageResult = changeCoverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = multipleImageChangeResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn)
    {
        logger.LogInformation("Partial updating of Workshops with ProviderId = {ProviderId} was started.", providerId);

        try
        {
            return await workshopRepository.UpdateProviderTitle(providerId, providerTitle, providerTitleEn).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError(exception,
                $"Partial updating {nameof(Workshop)} with ProviderId = {providerId} was failed. Exception: {exception.Message}");
            throw; // TODO Probably should not rethrow this exception to the higher level. See pull request [Provicevk/unified responses #843] as future decision
        }
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<IEnumerable<Workshop>> BlockByProvider(Provider provider)
    {
        logger.LogInformation($"Block {nameof(Workshop)} with ProviderId = {provider.Id} was started.");

        try
        {
            return await workshopRepository.BlockByProvider(provider).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError(exception,
                $"Block {nameof(Workshop)} with ProviderId = {provider.Id} was failed. Exception: {exception.Message}");
            throw; // TODO Probably should not rethrow this exception to the higher level. See pull request [Provicevk/unified responses #843] as future decision
        }
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task Delete(Guid id)
    {
        logger.LogInformation($"Deleting Workshop with Id = {id} started.");

        var entity = await workshopRepository.GetById(id).ConfigureAwait(false);
        try
        {
            await workshopRepository.Delete(entity).ConfigureAwait(false);
            logger.LogInformation($"Workshop with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Workshop with Id = {id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    /// <exception cref="InvalidOperationException">If unreal to delete images.</exception>
    public async Task DeleteV2(Guid id)
    {
        logger.LogInformation($"Deleting {nameof(Workshop)} with Id = {id} started.");

        async Task<Workshop> TransactionOperation()
        {
            var entity = await workshopRepository.GetWithNavigations(id).ConfigureAwait(false);

            if (entity.Images.Count > 0)
            {
                await workshopImagesService
                    .RemoveManyImagesAsync(entity, entity.Images.Select(x => x.ExternalStorageId).ToList())
                    .ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(entity.CoverImageId))
            {
                await workshopImagesService.RemoveCoverImageAsync(entity).ConfigureAwait(false);
            }

            foreach (var teacher in entity.Teachers.ToList())
            {
                await teacherService.Delete(teacher.Id).ConfigureAwait(false);
            }

            await workshopRepository.Delete(entity).ConfigureAwait(false);

            return null;
        }

        try
        {
            await workshopRepository.RunInTransaction(TransactionOperation).ConfigureAwait(false);
            logger.LogInformation($"{nameof(Workshop)} with Id = {id} successfully deleted.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, $"Deleting {nameof(Workshop)} with Id = {id} failed.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter = null)
    {
        logger.LogInformation("Getting Workshops by filter started.");

        filter ??= new WorkshopFilter();

        var filterPredicate = PredicateBuild(filter);
        var orderBy = GetOrderParameter(filter);

        var workshopsCount = await workshopRepository.Count(whereExpression: filterPredicate).ConfigureAwait(false);
        var workshops = workshopRepository.Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: includingPropertiesForMappingDtoModel,
                whereExpression: filterPredicate,
                orderBy: orderBy)
            .ToList();

        logger.LogInformation(!workshops.Any()
            ? "There was no matching entity found."
            : $"All matching {workshops.Count} records were successfully received from the Workshop table");

        var workshopCards = mapper.Map<List<WorkshopCard>>(workshops);

        var result = new SearchResult<WorkshopCard>()
        {
            TotalAmount = workshopsCount,
            Entities = await GetWorkshopsWithAverageRating(workshopCards).ConfigureAwait(false),
        };

        return result;
    }

    public async Task<SearchResult<WorkshopCard>> GetNearestByFilter(WorkshopFilter filter = null)
    {
        logger.LogInformation("Getting Workshops by filter started.");
        filter ??= new WorkshopFilter();

        var hash = default(GeoCoord).SetDegrees(filter.Latitude, filter.Longitude);
        var h3Location = Api.GeoToH3(hash, GeoMathHelper.Resolution);
        Api.KRing(h3Location, GeoMathHelper.KRingForResolution, out var neighbours);

        var filterPredicate = PredicateBuild(filter);

        var closestWorkshops = workshopRepository.Get(
                skip: 0,
                take: 0,
                includeProperties: includingPropertiesForMappingWorkShopCard,
                whereExpression: filterPredicate,
                orderBy: null)
            .Where(w => neighbours
                .Select(n => n.Value)
                .Any(hash => hash == w.Address.GeoHash));

        var workshopsCount = await closestWorkshops.CountAsync().ConfigureAwait(false);

        var enumerableWorkshops = closestWorkshops.AsEnumerable();

        var nearestWorkshops = enumerableWorkshops
            .Select(w => new
            {
                w,
                Distance = GeoMathHelper
                    .GetDistanceFromLatLonInKm(
                        w.Address.Latitude,
                        w.Address.Longitude,
                        (double)filter.Latitude,
                        (double)filter.Longitude),
            })
            .OrderBy(p => p.Distance).Take(filter.Size).Select(a => a.w);

        var workshopsDTO = mapper.Map<List<WorkshopCard>>(nearestWorkshops);

        var result = new SearchResult<WorkshopCard>()
        {
            TotalAmount = workshopsCount,
            Entities = workshopsDTO,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopDto>> FetchByFilterForAdmins(WorkshopFilterAdministration filter = null)
    {
        logger.LogInformation("Started retrieving Workshops by filter for admins.");

        if (filter == null)
        {
            logger.LogDebug(
                "Method {MethodName} started with null filter. Applying default {Filter}",
                nameof(FetchByFilterForAdmins),
                nameof(WorkshopFilterAdministration));

            filter = new WorkshopFilterAdministration();
        }

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

        var workshops = await workshopRepository.Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: includingPropertiesForMappingDtoModel,
                whereExpression: predicate,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        var workshopsCount = await workshopRepository
            .Count(predicate)
            .ConfigureAwait(false);

        logger.LogInformation("Retrieved {WorkshopsCount} matching records by filter for admins.", workshopsCount);

        var workshopsDTO = mapper.Map<List<WorkshopDto>>(workshops);

        return new SearchResult<WorkshopDto>()
        {
            TotalAmount = workshopsCount,
            Entities = workshopsDTO,
        };
    }

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

    private Expression<Func<Workshop, bool>> PredicateBuildForAdminds(
        WorkshopFilterAdministration filter,
        Guid adminInstitutionId,
        IEnumerable<long> allowedSettlementIdsForAdmin,
        IEnumerable<long> subSettlementFilterIds)
    {
        var predicate = PredicateBuilder.True<Workshop>();

        if (adminInstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => x.InstitutionHierarchy.InstitutionId == adminInstitutionId);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(x => x.InstitutionHierarchy.InstitutionId == filter.InstitutionId);
        }

        if (allowedSettlementIdsForAdmin != null && allowedSettlementIdsForAdmin.Any())
        {
            predicate = predicate.And(x => allowedSettlementIdsForAdmin.Contains(x.Address.CATOTTGId));
        }

        if (subSettlementFilterIds != null && subSettlementFilterIds.Any())
        {
            predicate = predicate.And(x => subSettlementFilterIds.Contains(x.Address.CATOTTGId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var searchTerms = searchStringService.SplitSearchString(filter.SearchString);

            if (searchTerms.Any())
            {
                var tempPredicate = PredicateBuilder.False<Workshop>();
                foreach (var word in searchTerms)
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.ShortTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.ProviderTitle.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.ProviderTitleEn.Contains(word, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase));
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        return predicate;
    }

    public async Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids)
    {
        return await workshopRepository.GetByIds(ids).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Guid> GetWorkshopProviderOwnerIdAsync(Guid workshopId)
    {
        return (await workshopRepository
            .GetByFilterNoTracking(w => w.Id == workshopId)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false)).ProviderId;
    }

    public async Task<bool> IsBlocked(Guid workshopId)
    {
        return (await workshopRepository.GetById(workshopId).ConfigureAwait(false)).IsBlocked;
    }

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) =>
        ModelValidationHelper.ValidateExcludedIdFilter(filter);

    private Expression<Func<Workshop, bool>> PredicateBuild(WorkshopFilter filter)
    {
        var predicate = PredicateBuilder.True<Workshop>();

        if (filter is WorkshopFilterWithSettlements settlementsFilter)
        {
            if (settlementsFilter.InstitutionId != Guid.Empty)
            {
                predicate = predicate.And(x => x.InstitutionHierarchy.InstitutionId == filter.InstitutionId);
            }

            if (settlementsFilter.SettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<Workshop>();

                foreach (var item in settlementsFilter.SettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Provider.LegalAddress.CATOTTGId == item);
                }

                predicate = predicate.And(tempPredicate);
            }
        }
        else
        {
            predicate = predicate.And(x => Provider.ValidProviderStatuses.Contains(x.Provider.Status));
            predicate = predicate.And(x => !x.IsBlocked);

            if (filter.CATOTTGId > 0)
            {
                predicate = predicate.And(x => x.Address.CATOTTGId == filter.CATOTTGId);
            }
        }

        if (filter.Ids.Any())
        {
            predicate = predicate.And(x => filter.Ids.Any(c => c == x.Id));

            return predicate;
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var tempPredicate = PredicateBuilder.False<Workshop>();

            // Fix Rider ambiguous method with either char or string args
            // ReSharper disable once UseCollectionExpression
            // ReSharper disable once RedundantExplicitArrayCreation
            foreach (var word in filter.SearchText.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(x => EF.Functions.Like(x.Keywords, $"%{word}%"));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.DirectionIds.Any())
        {
            var tempPredicate = PredicateBuilder.False<Workshop>();
            foreach (var direction in filter.DirectionIds)
            {
                tempPredicate = tempPredicate.Or(x => x.InstitutionHierarchy.Directions.Any(d => !d.IsDeleted && d.Id == direction));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.IsFree && (filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            predicate = predicate.And(x => x.Price == filter.MinPrice);
        }
        else if (!filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            predicate = predicate.And(x => x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice);
        }
        else if (filter.IsFree && !(filter.MinPrice == 0 && filter.MaxPrice == int.MaxValue))
        {
            predicate = predicate.And(x =>
                (x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice) || x.Price == 0);
        }

        if (filter.MinAge != 0 || filter.MaxAge != 100)
        {
            predicate = filter.IsAppropriateAge
                ? predicate.And(x => x.MinAge >= filter.MinAge && x.MaxAge <= filter.MaxAge)
                : predicate.And(x => x.MinAge <= filter.MaxAge && x.MaxAge >= filter.MinAge);
        }

        if (filter.WithDisabilityOptions)
        {
            predicate = predicate.And(x => x.WithDisabilityOptions);
        }

        if (filter.Workdays.Any())
        {
            var workdaysBitMask = filter.Workdays.Aggregate((prev, next) => prev | next);

            if (workdaysBitMask > 0)
            {
                predicate = filter.IsStrictWorkdays
                    ? predicate.And(x => x.DateTimeRanges.Any(tr => (tr.Workdays == workdaysBitMask)))
                    : predicate.And(x => x.DateTimeRanges.Any(tr => (tr.Workdays & workdaysBitMask) > 0));
            }
        }

        if (filter.MinStartTime.TotalMinutes > 0 || filter.MaxStartTime.Hours < 23)
        {
            predicate = filter.IsAppropriateHours
                ? predicate.And(x => x.DateTimeRanges.Any(tr =>
                    tr.StartTime >= filter.MinStartTime && tr.EndTime.Hours <= filter.MaxStartTime.Hours))
                : predicate.And(x => x.DateTimeRanges.Any(tr =>
                    tr.StartTime >= filter.MinStartTime && tr.StartTime.Hours <= filter.MaxStartTime.Hours));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            predicate = predicate.And(x => x.Address.CATOTTG.Name == filter.City);
        }

        if (filter.Statuses.Any())
        {
            predicate = predicate.And(x => filter.Statuses.Contains(x.Status));
        }

        if (filter.FormOfLearning.Any())
        {
            predicate = predicate.And(x => filter.FormOfLearning.Contains(x.FormOfLearning));
        }

        return predicate;
    }

    private Dictionary<Expression<Func<Workshop, object>>, SortDirection> GetOrderParameter(WorkshopFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Workshop, object>>, SortDirection>();

        switch (filter.OrderByField)
        {
            case nameof(OrderBy.Alphabet):
                sortExpression.Add(x => x.Title, SortDirection.Ascending);
                break;

            case nameof(OrderBy.PriceDesc):
                sortExpression.Add(x => x.Price, SortDirection.Descending);
                break;

            case nameof(OrderBy.PriceAsc):
                sortExpression.Add(x => x.Price, SortDirection.Ascending);
                break;

            default:
                sortExpression.Add(x => x.Id, SortDirection.Ascending);
                break;
        }

        return sortExpression;
    }

    private async Task<List<T>> GetWorkshopsWithAverageRating<T>(List<T> workshops)
        where T : WorkshopBaseCard
    {
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(workshops.Select(p => p.WorkshopId)).ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            var averageRatingDto = averageRatings?.SingleOrDefault(r => r.EntityId == workshop.WorkshopId);
            workshop.Rating = averageRatingDto?.Rate ?? default;
            workshop.NumberOfRatings = averageRatingDto?.RateQuantity ?? default;
        }

        return workshops;
    }

    private async Task<List<WorkshopDto>> GetWorkshopsWithAverageRating(List<WorkshopDto> workshops)
    {
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(workshops.Select(p => p.Id)).ConfigureAwait(false);

        foreach (var workshop in workshops)
        {
            var rating = averageRatings?.SingleOrDefault(r => r.EntityId == workshop.Id);
            workshop.Rating = rating?.Rate ?? default;
            workshop.NumberOfRatings = rating?.RateQuantity ?? default;
        }

        return workshops;
    }

    private async Task ChangeTeachers(Workshop currentWorkshop, List<TeacherDTO> teacherDtoList)
    {
        var deletedIds = currentWorkshop.Teachers
        .Where(x => !x.IsDeleted)
        .Select(x => x.Id)
        .Except(teacherDtoList.Select(x => x.Id))
        .ToList();

        var deleteTasks = deletedIds
            .Select(deletedId => teacherService.Delete(deletedId));

        await Task.WhenAll(deleteTasks).ConfigureAwait(false);

        foreach (var deletedId in deletedIds)
        {
            await teacherService.Delete(deletedId).ConfigureAwait(false);
        }

        foreach (var teacherDto in teacherDtoList)
        {
            if (currentWorkshop.Teachers.Select(x => x.Id).Contains(teacherDto.Id))
            {
                await teacherService.Update(teacherDto).ConfigureAwait(false);
            }
            else
            {
                var newTeacher = mapper.Map<TeacherDTO>(teacherDto);
                newTeacher.WorkshopId = currentWorkshop.Id;
                await teacherService.Create(newTeacher).ConfigureAwait(false);
            }
        }
    }

    private async Task UpdateDateTimeRanges(List<DateTimeRangeDto> dtos, Guid workshopId)
    {
        var ranges = mapper.Map<List<DateTimeRange>>(dtos);
        foreach (var range in ranges)
        {
            if (await dateTimeRangeRepository.Any(r => r.Id == range.Id).ConfigureAwait(false))
            {
                range.WorkshopId = workshopId;
                await dateTimeRangeRepository.Update(range).ConfigureAwait(false);
            }
        }
    }

    private async Task UpdateWorkshop()
    {
        try
        {
            await workshopRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, $"Updating a workshop failed. Exception: {ex.Message}");
            throw;
        }
    }

    private void ValidateOffsetFilter(OffsetFilter offsetFilter) => ModelValidationHelper.ValidateOffsetFilter(offsetFilter);

    private async Task UpdateWorkshopStatusBySeatsLimitAndAvailability(uint newAvailableSeats, Workshop currentWorkshop)
    {
        var currentWorkshopTakenSeats = currentWorkshop.Applications.TakenSeats();

        if (newAvailableSeats == uint.MaxValue
            && currentWorkshop.AvailableSeats == currentWorkshopTakenSeats
            && currentWorkshop.Status == WorkshopStatus.Closed)
        {
            await UpdateStatus(new()
            {
                WorkshopId = currentWorkshop.Id,
                Status = WorkshopStatus.Open,
            }).ConfigureAwait(false);
        }

        if (newAvailableSeats < uint.MaxValue
            && newAvailableSeats <= currentWorkshopTakenSeats
            && currentWorkshop.Status == WorkshopStatus.Open)
        {
            await UpdateStatus(new()
            {
                WorkshopId = currentWorkshop.Id,
                Status = WorkshopStatus.Closed,
            }).ConfigureAwait(false);
        }
    }

    private async Task<Workshop> CheckDtoAndPrepareCreatedWorkshop(WorkshopCreateRequestDto dto)
    {
        if (dto.MemberOfWorkshopId.HasValue && !await Exists((Guid)dto.MemberOfWorkshopId).ConfigureAwait(false))
        {
            var errorMessage = $"The main workshop (with id = {dto.MemberOfWorkshopId}) for the workshop being created was not found.";
            throw new InvalidOperationException(errorMessage);
        }

        if (dto.MemberOfWorkshopId.HasValue && (await workshopRepository.GetById((Guid)dto.MemberOfWorkshopId).ConfigureAwait(false)).MemberOfWorkshopId.HasValue)
        {
            var errorMessage = $"The main workshop (with ID = {dto.MemberOfWorkshopId}) for the workshop being created is a member of another workshop, so it cannot be the main workshop.";
            throw new InvalidOperationException(errorMessage);
        }

        dto.AvailableSeats = dto.AvailableSeats.GetMaxValueIfNullOrZero();
        Workshop createdWorkshop;

        if (dto is WorkshopV2CreateRequestDto v2Dto)
        {
            createdWorkshop = mapper.Map<Workshop>(v2Dto);
        }
        else
        {
            createdWorkshop = mapper.Map<Workshop>(dto);
        }

        createdWorkshop.Provider = await providerRepository.GetById(createdWorkshop.ProviderId).ConfigureAwait(false);
        createdWorkshop.ProviderOwnership = createdWorkshop.Provider.Ownership;
        createdWorkshop.ProviderTitle = createdWorkshop.Provider.FullTitle;
        createdWorkshop.ProviderTitleEn = createdWorkshop.Provider.FullTitleEn;

        if (!dto.Teachers.IsNullOrEmpty())
        {
            createdWorkshop.Teachers = dto.Teachers.Select(mapper.Map<Teacher>).ToList();
        }

        createdWorkshop.Tags = (await tagRepository.GetByFilter(tag => dto.TagIds.Contains(tag.Id))).ToList();
        createdWorkshop.Status = WorkshopStatus.Open;

        return createdWorkshop;
    }

    private async Task SetIdsToDefaultValue(WorkshopCreateRequestDto dto)
    {
        dto.Id = Guid.Empty;

        if (dto.Address is not null)
        {
            dto.Address.Id = default;
        }

        if (dto.DefaultTeacher is not null)
        {
            dto.DefaultTeacher.Id = Guid.Empty;
        }

        dto.WorkshopDescriptionItems?.ToList().ForEach(e => e.Id = Guid.Empty);
        dto.Teachers?.ToList().ForEach(e => e.Id = Guid.Empty);
        dto.DateTimeRanges?.ToList().ForEach(e => e.Id = default);

        // If the DefaultTeacherId property of WorkshopBaseDto is incorrect, throw InvalidOperationException.
        if (dto.DefaultTeacherId is not null && !await teacherService.ExistsAsync((Guid)dto.DefaultTeacherId).ConfigureAwait(false))
        {
            var errorMessage = $"The default Teacher (with id = {dto.DefaultTeacherId}) for the workshop being created was not found.";
            throw new InvalidOperationException(errorMessage);
        }
    }
}
