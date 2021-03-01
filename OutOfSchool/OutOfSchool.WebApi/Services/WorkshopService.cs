using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using NuGet.Frameworks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ResultModel;

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
        public async Task<Result<WorkshopDTO>> Create(WorkshopDTO dto)
        {
            try
            {
                var workshop = dto.ToDomain();

                var newWorkshop = await repository.Create(workshop).ConfigureAwait(false);

                return Result<WorkshopDTO>.GetSuccess(newWorkshop.ToModel());
            }
            catch
            {
                return Result<WorkshopDTO>.GetError(ErrorCode.InternalServerError, "Internal server error");
            }
        }

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<WorkshopDTO>>> GetAll()
        {
            var workshops = await repository.GetAll().ConfigureAwait(false);

            return Result<IEnumerable<WorkshopDTO>>.GetSuccess(
                workshops.Select(workshop => workshop.ToModel()).ToList());
        }

        public async Task<Result<WorkshopDTO>> GetById(long id)
        {
            var workshop = await repository.GetById(id).ConfigureAwait(false);

            if (workshop == null)
            {
                return Result<WorkshopDTO>.GetError(ErrorCode.NotFound, $"Workshop with id = {id} was not found.");
            }

            return Result<WorkshopDTO>.GetSuccess(workshop.ToModel());
        }

        public async Task<Result<WorkshopDTO>> Update(WorkshopDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return Result<WorkshopDTO>.GetError(ErrorCode.NotFound, "Workshop item is null.");
                }

                var workshop = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return Result<WorkshopDTO>.GetSuccess(workshop.ToModel());
            }
            catch
            {
                return Result<WorkshopDTO>.GetError(ErrorCode.InternalServerError, "Internal server error.");
            }
        }

        public async Task<Result<long>> Delete(long id)
        {
            try
            {
                var dtoToDelete = new WorkshopDTO() { Id = id };
                
                await repository.Delete(dtoToDelete.ToDomain()).ConfigureAwait(false);
                
                return Result<long>.GetSuccess(id);
            }
            catch 
            {
                return Result<long>.GetError(ErrorCode.NotFound, $"Workshop with id = {id} was not found.");
            }
        }
    }
}