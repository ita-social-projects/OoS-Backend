using System.Linq.Expressions;
using AutoMapper;
using Castle.Core.Internal;
using H3Lib;
using H3Lib.Extensions;
using Nest;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Workshop entity.
/// </summary>
public class WorkshopService : IWorkshopService
{
    private readonly string includingPropertiesForMappingDtoModel =
        $"{nameof(Workshop.Address)},{nameof(Workshop.Teachers)},{nameof(Workshop.DateTimeRanges)},{nameof(Workshop.InstitutionHierarchy)}";

    private readonly string includingPropertiesForMappingWorkShopCard = $"{nameof(Workshop.Address)}";

    private readonly IWorkshopRepository workshopRepository;
    private readonly IRatingService ratingService;
    private readonly ITeacherService teacherService;
    private readonly ILogger<WorkshopService> logger;
    private readonly IMapper mapper;
    private readonly IImageDependentEntityImagesInteractionService<Workshop> workshopImagesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopService"/> class.
    /// </summary>
    /// <param name="workshopRepository">Repository for Workshop entity.</param>
    /// <param name="ratingService">Rating service.</param>
    /// <param name="teacherService">Teacher service.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="workshopImagesService">Workshop images mediator.</param>
    public WorkshopService(
        IWorkshopRepository workshopRepository,
        IRatingService ratingService,
        ITeacherService teacherService,
        ILogger<WorkshopService> logger,
        IMapper mapper,
        IImageDependentEntityImagesInteractionService<Workshop> workshopImagesService)
    {
        this.workshopRepository = workshopRepository;
        this.ratingService = ratingService;
        this.teacherService = teacherService;
        this.logger = logger;
        this.mapper = mapper;
        this.workshopImagesService = workshopImagesService;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopDTO"/> is null.</exception>
    public async Task<WorkshopDTO> Create(WorkshopDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation("Workshop creating was started.");

        var workshop = mapper.Map<Workshop>(dto);
        if (dto.Teachers is not null)
        {
            workshop.Teachers = dto.Teachers.Select(dtoTeacher => mapper.Map<Teacher>(dtoTeacher)).ToList();
        }

        workshop.Status = WorkshopStatus.Open;

        Func<Task<Workshop>> operation = async () =>
            await workshopRepository.Create(workshop).ConfigureAwait(false);

        var newWorkshop = await workshopRepository.RunInTransaction(operation).ConfigureAwait(false);

        logger.LogInformation($"Workshop with Id = {newWorkshop?.Id} created successfully.");

        return mapper.Map<WorkshopDTO>(newWorkshop);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopDTO"/> is null.</exception>
    /// <exception cref="InvalidOperationException">If unreal to map teachers.</exception>
    /// <exception cref="DbUpdateException">If unreal to update entity.</exception>
    public async Task<WorkshopCreationResultDto> CreateV2(WorkshopDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation("Workshop creating was started.");

        async Task<(Workshop createdWorkshop, MultipleImageUploadingResult imagesUploadResult, Result<string>
            coverImageUploadResult)> CreateWorkshopAndDependencies()
        {
            var createdWorkshop = mapper.Map<Workshop>(dto);
            createdWorkshop.Status = WorkshopStatus.Open;
            var workshop = await workshopRepository.Create(createdWorkshop).ConfigureAwait(false);

            if (dto.Teachers != null)
            {
                foreach (var teacherDto in dto.Teachers)
                {
                    teacherDto.WorkshopId = workshop.Id;
                    await teacherService.Create(teacherDto).ConfigureAwait(false);
                }
            }

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

        return new WorkshopCreationResultDto
        {
            Workshop = mapper.Map<WorkshopDTO>(newWorkshop),
            UploadingCoverImageResult = coverImageUploadResult?.OperationResult,
            UploadingImagesResults = imagesUploadResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopDTO>> GetAll(OffsetFilter offsetFilter)
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

        var dtos = mapper.Map<List<WorkshopDTO>>(workshops);
        var workshopsWithRating = await GetWorkshopsWithAverageRating(dtos).ConfigureAwait(false);
        return new SearchResult<WorkshopDTO>() { TotalAmount = count, Entities = workshopsWithRating };
    }

    /// <inheritdoc/>
    public async Task<WorkshopDTO> GetById(Guid id)
    {
        logger.LogInformation($"Getting Workshop by Id started. Looking Id = {id}.");

        var workshop = await workshopRepository.GetWithNavigations(id).ConfigureAwait(false);

        if (workshop == null)
        {
            return null;
        }

        logger.LogInformation($"Successfully got a Workshop with Id = {id}.");

        var workshopDTO = mapper.Map<WorkshopDTO>(workshop);

        var rating = await ratingService.GetAverageRatingAsync(workshopDTO.Id, RatingType.Workshop)
            .ConfigureAwait(false);

        workshopDTO.Rating = rating?.Item1 ?? default;
        workshopDTO.NumberOfRatings = rating?.Item2 ?? default;

        return workshopDTO;
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetWorkshopListByProviderId(Guid providerId)
    {
        logger.LogDebug("Getting Workshop (Id, Title) by organization started. Looking ProviderId = {ProviderId}", providerId);

        var workshops = await workshopRepository.GetByFilter(
            predicate: x => x.ProviderId == providerId);

        var result = mapper.Map<List<ShortEntityDto>>(workshops).OrderBy(entity => entity.Title).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<T>> GetByProviderId<T>(Guid id, ExcludeIdFilter filter)
        where T : WorkshopBaseCard
    {
        logger.LogInformation($"Getting Workshop by organization started. Looking ProviderId = {id}.");

        filter ??= new ExcludeIdFilter();
        ValidateExcludedIdFilter(filter);

        var workshopBaseCardsCount = await workshopRepository.Count(where: x =>
                                                filter.ExcludedId == null ? (x.ProviderId == id)
                                                : (x.ProviderId == id && x.Id != filter.ExcludedId)).ConfigureAwait(false);
        var workshops = await workshopRepository.Get(
            skip: filter.From,
            take: filter.Size,
            includeProperties: includingPropertiesForMappingDtoModel,
            where: x => filter.ExcludedId == null ? (x.ProviderId == id)
                                  : (x.ProviderId == id && x.Id != filter.ExcludedId))
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!workshops.Any()
            ? $"There aren't Workshops for Provider with Id = {id}."
            : $"From Workshop table were successfully received {workshops.Count} records.");

        var workshopBaseCards = mapper.Map<List<T>>(workshops).ToList();
        var result = new SearchResult<T>()
        {
            TotalAmount = workshopBaseCardsCount,
            Entities = await GetWorkshopsWithAverageRating(workshopBaseCards).ConfigureAwait(false),
        };

        return result;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">If <see cref="WorkshopDTO"/> is null.</exception>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<WorkshopDTO> Update(WorkshopDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating Workshop with Id = {dto?.Id} started.");

        var currentWorkshop = await workshopRepository.GetWithNavigations(dto!.Id).ConfigureAwait(false);

        // In case if AddressId was changed. AddresId is one and unique for workshop.
        dto.AddressId = currentWorkshop.AddressId;
        dto.Address.Id = currentWorkshop.AddressId;

        mapper.Map(dto, currentWorkshop);
        currentWorkshop.Teachers = dto.Teachers?.Select(dtoTeacher => mapper.Map<Teacher>(dtoTeacher)).ToList();

        try
        {
            await workshopRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError($"Updating failed. Exception: {exception.Message}");
            throw;
        }

        return mapper.Map<WorkshopDTO>(currentWorkshop);
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
            if (currentWorkshop.AvailableSeats != uint.MaxValue)
            {
                currentWorkshop.Status = dto.Status;
            }
            else
            {
                logger.LogInformation(
                    $"Unable to update status for workshop(id) {dto.WorkshopId}. Number of seats has not restriction.");
                throw new ArgumentException(
                    "Unable to update status for workshop because of number of seats has not restriction.");
            }

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
    public async Task<WorkshopUpdateResultDto> UpdateV2(WorkshopDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating {nameof(Workshop)} with Id = {dto.Id} started.");

        async Task<(Workshop updatedWorkshop, MultipleImageChangingResult multipleImageChangingResult,
            ImageChangingResult changingCoverImageResult)> UpdateWorkshopWithDependencies()
        {
            var currentWorkshop = await workshopRepository.GetWithNavigations(dto.Id).ConfigureAwait(false);

            dto.ImageIds ??= new List<string>();
            var multipleImageChangingResult = await workshopImagesService
                .ChangeImagesAsync(currentWorkshop, dto.ImageIds, dto.ImageFiles)
                .ConfigureAwait(false);

            // In case if AddressId was changed. AddressId is one and unique for workshop.
            dto.AddressId = currentWorkshop.AddressId;
            dto.Address.Id = currentWorkshop.AddressId;

            await ChangeTeachers(currentWorkshop, dto.Teachers ?? new List<TeacherDTO>()).ConfigureAwait(false);

            mapper.Map(dto, currentWorkshop);

            var changingCoverImageResult = await workshopImagesService
                .ChangeCoverImageAsync(currentWorkshop, dto.CoverImageId, dto.CoverImage).ConfigureAwait(false);

            await UpdateWorkshop().ConfigureAwait(false);

            return (currentWorkshop, multipleImageChangingResult, changingCoverImageResult);
        }

        var (updatedWorkshop, multipleImageChangeResult, changeCoverImageResult) = await workshopRepository
            .RunInTransaction(UpdateWorkshopWithDependencies).ConfigureAwait(false);

        return new WorkshopUpdateResultDto
        {
            Workshop = mapper.Map<WorkshopDTO>(updatedWorkshop),
            UploadingCoverImageResult = changeCoverImageResult?.UploadingResult?.OperationResult,
            UploadingImagesResults = multipleImageChangeResult?.UploadedMultipleResult?.MultipleKeyValueOperationResult,
        };
    }

    /// <inheritdoc/>
    /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
    public async Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider)
    {
        logger.LogInformation($"Partial updating {nameof(Workshop)} with ProviderId = {provider?.Id} was started.");

        try
        {
            return await workshopRepository.PartialUpdateByProvider(provider).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError(exception,
                $"Partial updating {nameof(Workshop)} with ProviderId = {provider?.Id} was failed. Exception: {exception.Message}");
            throw;
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

        var workshopsCount = await workshopRepository.Count(where: filterPredicate).ConfigureAwait(false);
        var workshops = workshopRepository.Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: includingPropertiesForMappingDtoModel,
                where: filterPredicate,
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
                where: filterPredicate,
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

    private static void ValidateExcludedIdFilter(ExcludeIdFilter filter) => ModelValidationHelper.ValidateExcludedIdFilter(filter);

    private Expression<Func<Workshop, bool>> PredicateBuild(WorkshopFilter filter)
    {
        var predicate = PredicateBuilder.True<Workshop>();

        if (filter.Ids.Any())
        {
            predicate = predicate.And(x => filter.Ids.Any(c => c == x.Id));

            return predicate;
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var tempPredicate = PredicateBuilder.False<Workshop>();

            foreach (var word in filter.SearchText.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
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
                tempPredicate = tempPredicate.Or(x => x.InstitutionHierarchy.Directions.Any(d => d.Id == direction));
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

    private async Task<List<T>> GetWorkshopsWithAverageRating<T>(List<T> workshops) where T: WorkshopBaseCard
    {
        var averageRatings =
            await ratingService.GetAverageRatingForRangeAsync(workshops.Select(p => p.WorkshopId), RatingType.Workshop)
                .ConfigureAwait(false);

        if (averageRatings != null)
        {
            foreach (var workshop in workshops)
            {
                var ratingTuple = averageRatings.FirstOrDefault(r => r.Key == workshop.WorkshopId);
                workshop.Rating = ratingTuple.Value?.Item1 ?? default;
            }
        }

        return workshops;
    }

    private async Task<List<WorkshopDTO>> GetWorkshopsWithAverageRating(List<WorkshopDTO> workshops)
    {
        var averageRatings =
            await ratingService.GetAverageRatingForRangeAsync(workshops.Select(p => p.Id), RatingType.Workshop)
                .ConfigureAwait(false);

        if (averageRatings != null)
        {
            foreach (var workshop in workshops)
            {
                var ratingTuple = averageRatings.FirstOrDefault(r => r.Key == workshop.Id);
                workshop.Rating = ratingTuple.Value?.Item1 ?? default;
                workshop.NumberOfRatings = ratingTuple.Value?.Item2 ?? default;
            }
        }

        return workshops;
    }

    private async Task ChangeTeachers(Workshop currentWorkshop, List<TeacherDTO> teacherDtoList)
    {
        var deletedIds = currentWorkshop.Teachers.Select(x => x.Id).Except(teacherDtoList.Select(x => x.Id)).ToList();

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

    private async Task UpdateWorkshop()
    {
        try
        {
            await workshopRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, $"Updating a workshop failed. Exception: {ex.Message}");
            throw;
        }
    }

    private void ValidateOffsetFilter(OffsetFilter offsetFilter) => ModelValidationHelper.ValidateOffsetFilter(offsetFilter);
}