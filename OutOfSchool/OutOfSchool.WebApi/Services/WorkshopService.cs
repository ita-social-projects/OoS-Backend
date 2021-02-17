using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Workshop model.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private IEntityRepository<Workshop> WorkshopRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="mapper">Mapper instance.</param>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        public WorkshopService(IMapper mapper, IEntityRepository<Workshop> workshopRepository)
        {
            this.mapper = mapper;
            WorkshopRepository = workshopRepository;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO workshop)
        {
            if (workshop == null)
            {
                throw new ArgumentNullException($"{nameof(workshop)} entity must not be null");
            }

            try
            {
                var newWorkshop = mapper.Map<WorkshopDTO, Workshop>(workshop);

                var workshop_ = await WorkshopRepository.Create(newWorkshop).ConfigureAwait(false);

                return mapper.Map<Workshop, WorkshopDTO>(workshop_);
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
                var workshopDto = await Task.Run(() =>
                        WorkshopRepository.GetAll().Result
                            .Select(workshop => mapper.Map<Workshop, WorkshopDTO>(workshop)))
                    .ConfigureAwait(false);

                return workshopDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve workshops: {ex.Message}");
            }
        }

        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await WorkshopRepository.GetById(id).ConfigureAwait(false);

            if (workshop == null)
            {
                throw new NullReferenceException($"There is no {nameof(workshop)} with id = {id}.");
            }

            try
            {
                return mapper.Map<Workshop, WorkshopDTO>(workshop);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve workshop: {ex.Message}");
            }
        }

        public async Task<WorkshopDTO> Update(WorkshopDTO workshop)
        {
            if (workshop == null)
            {
                throw new ArgumentNullException($"{nameof(workshop)} was null.");
            }

            try
            {
                return mapper.Map<Workshop, WorkshopDTO>(await WorkshopRepository
                    .Update(mapper.Map<WorkshopDTO, Workshop>(workshop))
                    .ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(workshop)} could not be updated: {ex.Message}");
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                var workshopDto = await GetById(id).ConfigureAwait(false);

                await WorkshopRepository
                    .Delete(mapper.Map<WorkshopDTO, Workshop>(workshopDto))
                    .ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }
    }
}