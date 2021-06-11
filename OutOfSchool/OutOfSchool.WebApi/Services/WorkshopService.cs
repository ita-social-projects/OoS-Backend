﻿using System;
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
        private readonly ISubsubcategoryRepository subsubcategoryRepository;
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
        /// <param name="subsubcategoryRepository">Repository for Subsubcategory entity.</param>
        /// <param name="teacherRepository">Repository for Teacher.</param>
        /// <param name="addressRepository">Repository for Address.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopService(
            IWorkshopRepository workshopRepository,
            ISubsubcategoryRepository subsubcategoryRepository,
            IEntityRepository<Teacher> teacherRepository,
            IEntityRepository<Address> addressRepository,
            IRatingService ratingService,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.workshopRepository = workshopRepository;
            this.subsubcategoryRepository = subsubcategoryRepository;
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

            // In case if CategoryId and SubcategoryId does not match SubsubcategoryId
            await this.FillCategoriesFields(dto).ConfigureAwait(false);

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
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Workshop with Id = {id}.");

            var workshopDTO = workshop.ToModel();

            workshopDTO.Rating = ratingService.GetAverageRating(workshopDTO.Id, RatingType.Workshop);

            return workshopDTO;
        }

        public async Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id)
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

            var workshop = await workshopRepository.GetByFilterNoTracking(x => x.Id == dto.Id, "Address,Teachers").FirstAsync().ConfigureAwait(false);
            
            try
            {
                // In case if CategoryId and SubcategoryId does not match SubsubcategoryId
                await this.FillCategoriesFields(dto).ConfigureAwait(false);

                // In case if AddressId was changed. AddresId is one and unique for workshop.
                dto.AddressId = workshop.AddressId;
                dto.Address.Id = workshop.AddressId;

                // In case if WorkshopId of teachers was changed.
                foreach (var teacher in dto.Teachers)
                {
                    teacher.WorkshopId = workshop.Id;
                }

                // Sort new teachers and old teachers
                var oldTeachers = new List<TeacherDTO>();
                var newTeachers = new List<TeacherDTO>();
                foreach (var teacherDto in dto.Teachers)
                {
                    if (workshop.Teachers.Any(x => x.Id == teacherDto.Id))
                    {
                        oldTeachers.Add(teacherDto);
                    }
                    else
                    {
                        // In case if TeacherId was set wrong, when someone is trying to hack the system.
                        teacherDto.Id = default;
                        newTeachers.Add(teacherDto);
                    }
                }

                // Sort teachers for update and delete
                var updateTeachers = new List<TeacherDTO>();
                var deleteTeachers = new List<TeacherDTO>();
                foreach (var teacher in workshop.Teachers)
                {
                    if (oldTeachers.Any(x => x.Id == teacher.Id))
                    {
                        updateTeachers.Add(oldTeachers.Where(x => x.Id == teacher.Id).First());
                    }
                    else
                    {
                        deleteTeachers.Add(teacher.ToModel());
                    }
                }

                // When updating entity Workshop with the existing list
                // EF Core adds created and updated entities to the list so
                // when we return updated entity duplication happens.
                // This is only for returning entity. Database will be updated correctly.
                dto.Teachers = null;

                Func<Task<Workshop>> updateWorkshop = async () =>
                {
                    foreach (var teacherDto in newTeachers) { await teacherRepository.Create(teacherDto.ToDomain()).ConfigureAwait(false); }
                    foreach (var teacherDto in updateTeachers) { await teacherRepository.Update(teacherDto.ToDomain()).ConfigureAwait(false); }
                    foreach (var teacherDto in deleteTeachers) { await teacherRepository.Delete(teacherDto.ToDomain()).ConfigureAwait(false); }

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
                await workshopRepository.Delete(entity).ConfigureAwait(false);

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

            if (filter.Categories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var category in filter.Categories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Subsubcategory.Subcategory.Category.Title == category);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Subcategories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var subcategory in filter.Subcategories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Subsubcategory.Subcategory.Title == subcategory);
                }

                predicate = predicate.And(tempPredicate);
            }

            if (filter.Subsubcategories != null)
            {
                var tempPredicate = PredicateBuilder.True<Workshop>();
                foreach (var subsubcategory in filter.Subsubcategories)
                {
                    tempPredicate = tempPredicate.Or(x => x.Subsubcategory.Title == subsubcategory);
                }

                predicate = predicate.And(tempPredicate);
            }

            return predicate;
        }

        private async Task FillCategoriesFields(WorkshopDTO dto)
        {
            var sscategory = await subsubcategoryRepository.GetById(dto.SubsubcategoryId).ConfigureAwait(false);
            if (sscategory is null)
            {
                throw new ArgumentException($"There is no Subsubcategory with id:{dto.SubsubcategoryId}");
            }

            dto.SubcategoryId = sscategory.SubcategoryId;
            dto.CategoryId = sscategory.Subcategory.CategoryId;
        }
    }
}