using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
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
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IPaginationHelper<Workshop> paginationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        /// <param name="classRepository">Repository for Class entity.</param>
        /// <param name="teacherRepository">Repository for Teacher.</param>
        /// <param name="addressRepository">Repository for Address.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopService(
            IWorkshopRepository workshopRepository,
            IClassRepository classRepository,
            IEntityRepository<Teacher> teacherRepository,
            IEntityRepository<Address> addressRepository,
            IRatingService ratingService,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.workshopRepository = workshopRepository;
            this.classRepository = classRepository;
            this.teacherRepository = teacherRepository;
            this.addressRepository = addressRepository;
            this.ratingService = ratingService;
            this.logger = logger;
            this.localizer = localizer;
            this.paginationHelper = new PaginationHelper<Workshop>(workshopRepository);
        }

        /// <inheritdoc/>
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
        public async Task<IEnumerable<WorkshopDTO>> GetAll()
        {
            logger.Information("Getting all Workshops started.");

            var workshops = await workshopRepository.GetAll().ConfigureAwait(false);

            logger.Information(!workshops.Any()
                ? "Workshop table is empty."
                : $"All {workshops.Count()} records were successfully received from the Workshop table");

            var workshopsDTO = workshops.Select(x => x.ToModel()).ToList();

            var averageRatings = ratingService.GetAverageRatingForRange(workshopsDTO.Select(p => p.Id), RatingType.Workshop);

            if (averageRatings != null)
            {
                foreach (var workshop in workshopsDTO)
                {
                    workshop.Rating = averageRatings.FirstOrDefault(r => r.Key == workshop.Id).Value;
                }
            }

            return workshopsDTO;
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

            workshopDTO.Rating = ratingService.GetAverageRating(workshopDTO.Id, RatingType.Workshop);

            return workshopDTO;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsByProviderId(long id)
        {
            logger.Information($"Getting Workshop by organization started. Looking ProviderId = {id}.");

            var workshops = await workshopRepository.GetByFilter(x => x.Provider.Id == id).ConfigureAwait(false);

            logger.Information(!workshops.Any()
                ? $"There aren't Workshops for Provider with Id = {id}."
                : $"From Workshop table were successfully received {workshops.Count()} records.");

            return workshops.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
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
        public async Task<int> GetPagesCount(WorkshopFilter filter, int size)
        {
            PaginationValidation(filter);
            var predicate = PredicateBuild(filter);
            return await paginationHelper.GetCountOfPages(size, predicate).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber)
        {
            PaginationValidation(filter);
            var predicate = PredicateBuild(filter);

            bool ascending = filter.OrderByPriceAscending;
            Expression<Func<Workshop, decimal>> orderBy = x => x.Price;

            var page = await paginationHelper.GetPage(pageNumber, size, null, predicate, orderBy, ascending).ConfigureAwait(false);

            return page.Select(x => x.ToModel()).ToList();
        }

        private void PaginationValidation(WorkshopFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentException(localizer["The filter cannot be null"]);
            }
        }

        private Expression<Func<Workshop, bool>> PredicateBuild(WorkshopFilter filter)
        {
            var predicate = PredicateBuilder.True<Workshop>();

            if (!string.IsNullOrEmpty(filter.SearchFieldText))
            {
                predicate = predicate.And(x => x.Title.Contains(filter.SearchFieldText, StringComparison.CurrentCulture));
            }

            if (filter.Age != 0)
            {
                predicate = predicate.And(x => (x.MinAge <= filter.Age) && (x.MaxAge >= filter.Age));
            }

            if (filter.DaysPerWeek != 0)
            {
                predicate = predicate.And(x => x.DaysPerWeek == filter.DaysPerWeek);
            }

            predicate = predicate.And(x => x.Price >= filter.MinPrice);

            if (filter.MaxPrice != 0)
            {
                predicate = predicate.And(x => x.Price <= filter.MaxPrice);
            }

            predicate = predicate.And(x => x.WithDisabilityOptions == filter.Disability);

            if (filter.Directions != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var direction in filter.Directions)
                {
                    tempPredicate = tempPredicate.Or(x => x.Class.Department.Direction.Title == direction);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Departments != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var department in filter.Departments)
                {
                    tempPredicate = tempPredicate.Or(x => x.Class.Department.Title == department);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Classes != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var classEntity in filter.Classes)
                {
                    tempPredicate = tempPredicate.Or(x => x.Class.Title == classEntity);
                }

                predicate = predicate.And(tempPredicate);
            }

            return predicate;
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
    }
}