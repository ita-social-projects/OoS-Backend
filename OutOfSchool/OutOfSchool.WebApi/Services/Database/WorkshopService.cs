using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private readonly IWorkshopRepository workshopRepository;
        private readonly IClassRepository classRepository;
        private readonly IEntityRepository<Teacher> teacherRepository;
        private readonly IEntityRepository<Address> addressRepository;
        private readonly IRatingService ratingService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        /// <param name="classRepository">Repository for Class entity.</param>
        /// <param name="teacherRepository">Repository for Teacher.</param>
        /// <param name="addressRepository">Repository for Address.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        public WorkshopService(
            IWorkshopRepository workshopRepository,
            IClassRepository classRepository,
            IEntityRepository<Teacher> teacherRepository,
            IEntityRepository<Address> addressRepository,
            IRatingService ratingService,
            ILogger logger)
        {
            this.workshopRepository = workshopRepository;
            this.classRepository = classRepository;
            this.teacherRepository = teacherRepository;
            this.addressRepository = addressRepository;
            this.ratingService = ratingService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">If any inner entities were not found.</exception>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            logger.Information("Workshop creating was started.");

            // In case if DirectionId and DepartmentId does not match ClassId
            await this.FillDirectionsFields(dto).ConfigureAwait(false);

            Func<Task<Workshop>> operation = async () => await workshopRepository.Create(dto.ToDomain()).ConfigureAwait(false);

            var newWorkshop = await workshopRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.Information($"Workshop with Id = {newWorkshop?.Id} created successfully.");

            return newWorkshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopDTO>> GetAll(OffsetFilter offsetFilter)
        {
            logger.Information("Getting all Workshops started.");

            if (offsetFilter is null)
            {
                offsetFilter = new OffsetFilter();
            }

            var count = await workshopRepository.Count().ConfigureAwait(false);
            var workshops = workshopRepository.Get<long>(skip: offsetFilter.From, take: offsetFilter.Size, orderBy: x => x.Id, ascending: true).ToList();

            logger.Information(!workshops.Any()
                ? "Workshop table is empty."
                : $"All {workshops.Count} records were successfully received from the Workshop table");

            var workshopsDTO = workshops.Select(x => x.ToModel()).ToList();
            var workshopsWithRating = GetWorkshopsWithAverageRating(workshopsDTO);
            return new SearchResult<WorkshopDTO>() { TotalAmount = count, Entities = workshopsWithRating };
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            logger.Information($"Getting Workshop by Id started. Looking Id = {id}.");

            var workshop = await workshopRepository.GetById(id).ConfigureAwait(false);

            if (workshop == null)
            {
                return null;
            }

            logger.Information($"Successfully got a Workshop with Id = {id}.");

            var workshopDTO = workshop.ToModel();

            var rating = ratingService.GetAverageRating(workshopDTO.Id, RatingType.Workshop);

            workshopDTO.Rating = rating?.Item1 ?? default;
            workshopDTO.NumberOfRatings = rating?.Item2 ?? default;

            return workshopDTO;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id)
        {
            logger.Information($"Getting Workshop by organization started. Looking ProviderId = {id}.");

            var workshops = await workshopRepository.GetByFilter(x => x.ProviderId == id).ConfigureAwait(false);

            logger.Information(!workshops.Any()
                ? $"There aren't Workshops for Provider with Id = {id}."
                : $"From Workshop table were successfully received {workshops.Count()} records.");

            var workshopsDTO = workshops.Select(x => x.ToModel()).ToList();

            return GetWorkshopsWithAverageRating(workshopsDTO);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException">If the workshop was not found. Or if any inner entities were not found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            logger.Information($"Updating Workshop with Id = {dto?.Id} started.");

            var workshop = workshopRepository.GetByFilterNoTracking(x => x.Id == dto.Id, "Address,Teachers").FirstOrDefault();

            if (workshop is null)
            {
                throw new ArgumentOutOfRangeException($"The workshop with id:{dto.Id} was not found.");
            }

            try
            {
                // In case if DirectionId and DepartmentId does not match ClassId
                await this.FillDirectionsFields(dto).ConfigureAwait(false);

                // In case if AddressId was changed. AddresId is one and unique for workshop.
                dto.AddressId = workshop.AddressId;
                dto.Address.Id = workshop.AddressId;

                // In case if WorkshopId of teachers was changed.
                foreach (var teacher in dto.Teachers)
                {
                    teacher.WorkshopId = workshop.Id;
                }

                this.CompareTwoListsOfTeachers(
                    dto.Teachers,
                    workshop.Teachers,
                    out List<TeacherDTO> teachersToCreate,
                    out List<TeacherDTO> teachersToUpdate,
                    out List<TeacherDTO> teachersToDelete);

                // When updating entity Workshop with the existing list
                // EF Core adds created and updated entities to the list so
                // when we return updated entity duplication happens.
                // This is only for returning entity. Database will be updated correctly.
                dto.Teachers = null;

                Func<Task<Workshop>> updateWorkshop = async () =>
                {
                    foreach (var teacherDto in teachersToCreate)
                    {
                        await teacherRepository.Create(teacherDto.ToDomain()).ConfigureAwait(false);
                    }

                    foreach (var teacherDto in teachersToUpdate)
                    {
                        await teacherRepository.Update(teacherDto.ToDomain()).ConfigureAwait(false);
                    }

                    foreach (var teacherDto in teachersToDelete)
                    {
                        await teacherRepository.Delete(teacherDto.ToDomain()).ConfigureAwait(false);
                    }

                    await addressRepository.Update(dto.Address.ToDomain()).ConfigureAwait(false);
                    return await workshopRepository.Update(dto.ToDomain()).ConfigureAwait(false);
                };

                var newWorkshop = await workshopRepository.RunInTransaction(updateWorkshop).ConfigureAwait(false);

                logger.Information($"Workshop with Id = {workshop?.Id} updated succesfully.");

                return newWorkshop.ToModel();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Updating failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">If the entity with specified id was not found in the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Workshop with Id = {id} started.");

            var entity = await workshopRepository.GetById(id).ConfigureAwait(false);

            try
            {
                Func<Task<Workshop>> deleteWorkshop = async () =>
                {
                    await workshopRepository.Delete(entity).ConfigureAwait(false);
                    return new Workshop() { Id = default };
                };

                await workshopRepository.RunInTransaction(deleteWorkshop).ConfigureAwait(false);

                logger.Information($"Workshop with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Workshop with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopDTO>> GetByFilter(WorkshopFilter filter = null)
        {
            logger.Information("Getting Workshops by filter started.");

            if (filter is null)
            {
                filter = new WorkshopFilter();
            }

            var filterPredicate = PredicateBuild(filter);
            var orderBy = GetOrderParameter(filter);

            var workshopsCount = await workshopRepository.Count(where: filterPredicate).ConfigureAwait(false);
            var workshops = workshopRepository.Get<dynamic>(filter.From, filter.Size, string.Empty, filterPredicate, orderBy.Item1, orderBy.Item2).ToList();

            logger.Information(!workshops.Any()
                ? "There was no matching entity found."
                : $"All matching {workshops.Count} records were successfully received from the Workshop table");

            var workshopsDTO = workshops.Select(x => x.ToModel()).ToList();

            var result = new SearchResult<WorkshopDTO>()
            {
                TotalAmount = workshopsCount,
                Entities = GetWorkshopsWithAverageRating(workshopsDTO),
            };

            return result;
        }

        private Expression<Func<Workshop, bool>> PredicateBuild(WorkshopFilter filter)
        {
            var predicate = PredicateBuilder.True<Workshop>();

            if (!(filter.Ids is null) && filter.Ids.Count > 0)
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

            if (filter.DirectionIds[0] != 0)
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
                predicate = predicate.And(x => (x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice) || x.Price == 0);
            }

            if (filter.MinAge != 0 || filter.MaxAge != 100)
            {
                predicate = predicate.And(x => x.MinAge <= filter.MaxAge && x.MaxAge >= filter.MinAge);
            }

            if (filter.WithDisabilityOptions)
            {
                predicate = predicate.And(x => x.WithDisabilityOptions);
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
                throw new ArgumentOutOfRangeException($"There is no Class with id:{dto.ClassId}");
            }

            dto.DepartmentId = classEntity.DepartmentId;
            dto.DirectionId = classEntity.Department.DirectionId;
        }

        private void CompareTwoListsOfTeachers(
            IEnumerable<TeacherDTO> source,
            IEnumerable<Teacher> destination,
            out List<TeacherDTO> teachersToCreate,
            out List<TeacherDTO> teachersToUpdate,
            out List<TeacherDTO> teachersToDelete)
        {
            // Sort new teachers and old teachers
            var oldTeachers = new List<TeacherDTO>();
            teachersToCreate = new List<TeacherDTO>();
            foreach (var teacherDto in source)
            {
                if (destination.Any(x => x.Id == teacherDto.Id))
                {
                    oldTeachers.Add(teacherDto);
                }
                else
                {
                    // In case if TeacherId was set wrong, when someone is trying to hack the system.
                    teacherDto.Id = default;
                    teachersToCreate.Add(teacherDto);
                }
            }

            // Sort teachers for update and delete
            teachersToUpdate = new List<TeacherDTO>();
            teachersToDelete = new List<TeacherDTO>();
            foreach (var teacher in destination)
            {
                if (oldTeachers.Any(x => x.Id == teacher.Id))
                {
                    teachersToUpdate.Add(oldTeachers.First(x => x.Id == teacher.Id));
                }
                else
                {
                    teachersToDelete.Add(teacher.ToModel());
                }
            }
        }

        private List<WorkshopDTO> GetWorkshopsWithAverageRating(List<WorkshopDTO> workshopsDTOs)
        {
            var averageRatings = ratingService.GetAverageRatingForRange(workshopsDTOs.Select(p => p.Id), RatingType.Workshop);

            if (averageRatings != null)
            {
                foreach (var workshop in workshopsDTOs)
                {
                    var ratingTuple = averageRatings.FirstOrDefault(r => r.Key == workshop.Id);
                    workshop.Rating = ratingTuple.Value?.Item1 ?? default;
                    workshop.NumberOfRatings = ratingTuple.Value?.Item2 ?? default;
                }
            }

            return workshopsDTOs;
        }
    }
}