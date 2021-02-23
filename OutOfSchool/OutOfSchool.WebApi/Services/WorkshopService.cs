using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NuGet.Frameworks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Workshop model.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private IEntityRepository<Workshop> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        public WorkshopService(EntityRepository<Workshop> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO workshopDto)
        {
            if (workshopDto == null)
            {
                throw new ArgumentNullException($"{nameof(workshopDto)} entity must not be null");
            }

            try
            {
                var workshop = workshopDto.ToDomain();

                var newWorkshop = await _repository.Create(workshop).ConfigureAwait(false);

                return newWorkshop.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(WorkshopDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetAll()
        {
            try
            {
                var workshops = await _repository.GetAll().ConfigureAwait(false);

                var workshopsDto = workshops.Select(workshop => workshop.ToModel());

                return workshopsDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve workshops: {ex.Message}");
            }
        }

        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await _repository.GetById(id).ConfigureAwait(false);

            if (workshop == null)
            {
                throw new NullReferenceException($"There is no {nameof(workshop)} with id = {id}.");
            }

            try
            {
                return workshop.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve workshopDto: {ex.Message}");
            }
        }

        public async Task<WorkshopDTO> Update(WorkshopDTO workshopDto)
        {
            if (workshopDto == null)
            {
                throw new ArgumentNullException($"{nameof(workshopDto)} was null.");
            }

            try
            {
                var workshop = await _repository.Update(workshopDto.ToDomain()).ConfigureAwait(false);

                return workshop.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(workshopDto)} could not be updated: {ex.Message}");
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                var workshopDto = await GetById(id).ConfigureAwait(false);

                await _repository
                    .Delete(workshopDto.ToDomain())
                    .ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }
    }
}