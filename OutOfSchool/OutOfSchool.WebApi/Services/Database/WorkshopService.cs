using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using H3Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private readonly string includingPropertiesForMappingDtoModel = $"{nameof(Workshop.Address)},{nameof(Workshop.Teachers)},{nameof(Workshop.DateTimeRanges)},{nameof(Workshop.Direction)}";
        private readonly string includingPropertiesForMappingWorkShopCard = $"{nameof(Workshop.Address)}";

        private readonly IWorkshopRepository workshopRepository;
        private readonly IClassRepository classRepository;
        private readonly IRatingService ratingService;
        private readonly ILogger<WorkshopService> logger;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        /// <param name="classRepository">Repository for Class entity.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Automapper DI service.</param>
        public WorkshopService(
            IWorkshopRepository workshopRepository,
            IClassRepository classRepository,
            IRatingService ratingService,
            ILogger<WorkshopService> logger,
            IMapper mapper)
        {
            this.workshopRepository = workshopRepository;
            this.classRepository = classRepository;
            this.ratingService = ratingService;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">If any inner entities were not found.</exception>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            logger.LogInformation("Workshop creating was started.");

            // In case if DirectionId and DepartmentId does not match ClassId
            await this.FillDirectionsFields(dto).ConfigureAwait(false);

            Func<Task<Workshop>> operation = async () =>
                await workshopRepository.Create(mapper.Map<Workshop>(dto)).ConfigureAwait(false);

            var newWorkshop = await workshopRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogInformation($"Workshop with Id = {newWorkshop?.Id} created successfully.");

            return mapper.Map<WorkshopDTO>(newWorkshop);
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopDTO>> GetAll(OffsetFilter offsetFilter)
        {
            logger.LogInformation("Getting all Workshops started.");

            offsetFilter ??= new OffsetFilter();

            var count = await workshopRepository.Count().ConfigureAwait(false);
            var workshops =
                workshopRepository.Get(
                    skip: offsetFilter.From,
                    take: offsetFilter.Size,
                    includeProperties: includingPropertiesForMappingDtoModel,
                    orderBy: x => x.Id,
                    ascending: true)
                .ToList();

            logger.LogInformation(!workshops.Any()
                ? "Workshop table is empty."
                : $"All {workshops.Count} records were successfully received from the Workshop table");

            var dtos = mapper.Map<List<WorkshopDTO>>(workshops);
            var workshopsWithRating = GetWorkshopsWithAverageRating(dtos);
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

            var rating = ratingService.GetAverageRating(workshopDTO.Id, RatingType.Workshop);

            workshopDTO.Rating = rating?.Item1 ?? default;
            workshopDTO.NumberOfRatings = rating?.Item2 ?? default;

            return workshopDTO;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopCard>> GetByProviderId(Guid id)
        {
            logger.LogInformation($"Getting Workshop by organization started. Looking ProviderId = {id}.");

            var workshops = await workshopRepository.GetByFilter(x => x.ProviderId == id, includingPropertiesForMappingDtoModel).ConfigureAwait(false);

            logger.LogInformation(!workshops.Any()
                ? $"There aren't Workshops for Provider with Id = {id}."
                : $"From Workshop table were successfully received {workshops.Count()} records.");

            var cards = mapper.Map<List<WorkshopCard>>(workshops);

            return cards;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">If the workshop was not found. Or if any inner entities were not found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            logger.LogInformation($"Updating Workshop with Id = {dto?.Id} started.");

            // In case if DirectionId and DepartmentId does not match ClassId
            await this.FillDirectionsFields(dto).ConfigureAwait(false);

            var currentWorkshop = await workshopRepository.GetWithNavigations(dto!.Id).ConfigureAwait(false);

            // In case if AddressId was changed. AddresId is one and unique for workshop.
            dto.AddressId = currentWorkshop.AddressId;
            dto.Address.Id = currentWorkshop.AddressId;

            mapper.Map(dto, currentWorkshop);
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
        /// <exception cref="ArgumentNullException">If the entity with specified id was not found in the database.</exception>
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
        public async Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter = null)
        {
            logger.LogInformation("Getting Workshops by filter started.");

            if (filter is null)
            {
                filter = new WorkshopFilter();
            }

            var filterPredicate = PredicateBuild(filter);
            var orderBy = GetOrderParameter(filter);

            var workshopsCount = await workshopRepository.Count(where: filterPredicate).ConfigureAwait(false);
            var workshops = workshopRepository.Get<dynamic>(
                skip: filter.From,
                take: filter.Size,
                includeProperties: includingPropertiesForMappingDtoModel,
                where: filterPredicate,
                orderBy: orderBy.Item1,
                ascending: orderBy.Item2)
                .ToList();

            logger.LogInformation(!workshops.Any()
                ? "There was no matching entity found."
                : $"All matching {workshops.Count} records were successfully received from the Workshop table");

            var workshopCards = mapper.Map<List<WorkshopCard>>(workshops);

            var result = new SearchResult<WorkshopCard>()
            {
                TotalAmount = workshopsCount,
                Entities = GetWorkshopsWithAverageRating(workshopCards),
            };

            return result;
        }

        public async Task<SearchResult<WorkshopCard>> GetNearestByFilter(WorkshopFilter filter = null)
        {
            logger.LogInformation("Getting Workshops by filter started.");
            if (filter is null)
            {
                filter = new WorkshopFilter();
            }

            var geo = new GeoCoord(filter.Latitude, filter.Longitude);
            var h3Location = H3Lib.Api.GeoToH3(geo, GeoMathHelper.Resolution);
            Api.KRing(h3Location, GeoMathHelper.KRingForResolution, out var neighbours);

            var filterPredicate = PredicateBuild(filter);

            var closestWorkshops = workshopRepository.Get<dynamic>(
                skip: 0,
                take: 0,
                includeProperties: includingPropertiesForMappingWorkShopCard,
                where: filterPredicate,
                orderBy: null,
                ascending: true)
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
                            (double)geo.Latitude,
                            (double)geo.Longitude),
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

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsForCreate()
        {
            var workshops = await workshopRepository.GetListOfWorkshopsForSynchronizationByOperation(ElasticsearchSyncOperation.Create).ConfigureAwait(false);

            return workshops.Select(x => x.ToModel()).ToList();
        }

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsForUpdate()
        {
            var workshops = await workshopRepository.GetListOfWorkshopsForSynchronizationByOperation(ElasticsearchSyncOperation.Update).ConfigureAwait(false);

            return workshops.Select(x => x.ToModel()).ToList();
        }

        public async Task<IEnumerable<long>> GetWorkshopsForDelete()
        {
            var workshopIds = await workshopRepository.GetListOfWorkshopIdsForSynchronizationByOperation(ElasticsearchSyncOperation.Delete).ConfigureAwait(false);

            return workshopIds;
        }

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
                    tempPredicate = tempPredicate.Or(x => x.DirectionId == direction);
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
                predicate = predicate.And(x => x.MinAge <= filter.MaxAge && x.MaxAge >= filter.MinAge);
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
                    predicate = predicate.And(x => x.DateTimeRanges.Any(tr => (tr.Workdays & workdaysBitMask) > 0));
                }
            }

            if (filter.MinStartTime.TotalMinutes > 0 || filter.MaxStartTime.Hours < 23)
            {
                predicate = predicate.And(x => x.DateTimeRanges.Any(tr =>
                    tr.StartTime >= filter.MinStartTime
                        && tr.StartTime.Hours <= filter.MaxStartTime.Hours));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                predicate = predicate.And(x => x.Address.City == filter.City);
            }

            return predicate;
        }

        private Tuple<Expression<Func<Workshop, dynamic>>, bool> GetOrderParameter(WorkshopFilter filter)
        {
            switch (filter.OrderByField)
            {
                case nameof(OrderBy.Alphabet):
                    Expression<Func<Workshop, dynamic>> orderByAlphabet = x => x.Title;
                    var alphabetIsAscending = true;
                    return Tuple.Create(orderByAlphabet, alphabetIsAscending);

                case nameof(OrderBy.PriceDesc):
                    Expression<Func<Workshop, dynamic>> orderByPriceDesc = x => x.Price;
                    var priceIsAsc = false;
                    return Tuple.Create(orderByPriceDesc, priceIsAsc);

                case nameof(OrderBy.PriceAsc):
                    Expression<Func<Workshop, dynamic>> orderByPriceAsc = x => x.Price;
                    var priceIsAsce = true;
                    return Tuple.Create(orderByPriceAsc, priceIsAsce);

                default:
                    Expression<Func<Workshop, dynamic>> orderBy = x => x.Id;
                    var isAscending = true;
                    return Tuple.Create(orderBy, isAscending);
            }
        }

        /// <summary>
        /// Set properties of the given WorkshopDTO with certain data: DirectionId and DepartmentId are set with data according to the found Class entity.
        /// </summary>
        /// <param name="dto">WorkshopDTO to fill.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">If Class was not found.</exception>
        private async Task FillDirectionsFields(WorkshopDTO dto)
        {
            var classEntity = await classRepository.GetById(dto.ClassId).ConfigureAwait(false);
            if (classEntity is null)
            {
                throw new ArgumentException($"There is no Class with id:{dto.ClassId}");
            }

            dto.DepartmentId = classEntity.DepartmentId;
            dto.DirectionId = classEntity.Department.DirectionId;
        }

        private List<WorkshopCard> GetWorkshopsWithAverageRating(List<WorkshopCard> workshops)
        {
            var averageRatings =
                ratingService.GetAverageRatingForRange(workshops.Select(p => p.WorkshopId), RatingType.Workshop);

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

        private List<WorkshopDTO> GetWorkshopsWithAverageRating(List<WorkshopDTO> workshops)
        {
            var averageRatings =
                ratingService.GetAverageRatingForRange(workshops.Select(p => p.Id), RatingType.Workshop);

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
    }
}
