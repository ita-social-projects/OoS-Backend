using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Components.Web;
using NuGet.Frameworks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private IEntityRepository<Workshop> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        public WorkshopService(IEntityRepository<Workshop> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException($"{nameof(dto)} entity must not be null");
            }

            try
            {
                var workshop = dto.ToDomain();

                var newWorkshop = await repository.Create(workshop).ConfigureAwait(false);

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
                var workshops = await repository.GetAll().ConfigureAwait(false);

                return workshops.Select(workshop => workshop.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Workshops: {ex.Message}");
            }
        }

        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await repository.GetById(id).ConfigureAwait(false);

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

        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException($"{nameof(dto)} was null.");
            }

            try
            {
                var workshop = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return workshop.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(dto)} could not be updated: {ex.Message}");
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                await repository
                    .Delete(await repository.GetById(id).ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }
    }
}