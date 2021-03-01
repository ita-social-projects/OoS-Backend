using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ResultModel;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private IEntityRepository<Child> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the Child entity.</param>
        public ChildService(IEntityRepository<Child> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<Result<ChildDTO>> Create(ChildDTO dto)
        {
            try
            {
                var child = dto.ToDomain();

                var newChild = await repository.Create(child).ConfigureAwait(false);

                return Result<ChildDTO>.GetSuccess(newChild.ToModel());
            }
            catch
            {
                return Result<ChildDTO>.GetError(ErrorCode.InternalServerError, "Internal server error");
            }
        }

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<ChildDTO>>> GetAll()
        {
            var children = await repository.GetAll().ConfigureAwait(false);

            return Result<IEnumerable<ChildDTO>>.GetSuccess(children.Select(child => child.ToModel()).ToList());
        }

        /// <inheritdoc/>
        public async Task<Result<ChildDTO>> GetById(long id)
        {
            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                return Result<ChildDTO>.GetError(ErrorCode.NotFound, $"Child with id = {id} was not found.");
            }

            return Result<ChildDTO>.GetSuccess(child.ToModel());
        }

        /// <inheritdoc/>
        public async Task<Result<ChildDTO>> Update(ChildDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return Result<ChildDTO>.GetError(ErrorCode.NotFound, "Child item is null.");
                }

                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return Result<ChildDTO>.GetSuccess(child.ToModel());
            }
            catch
            {
                return Result<ChildDTO>.GetError(ErrorCode.InternalServerError, "Internal server error.");
            }
        }

        /// <inheritdoc/>
        public async Task<Result<long>> Delete(long id)
        {
            try
            {
                var dtoToDelete = new ChildDTO() { Id = id };
                
                await repository.Delete(dtoToDelete.ToDomain()).ConfigureAwait(false);
                
                return Result<long>.GetSuccess(id);
            }
            catch
            {
                return Result<long>.GetError(ErrorCode.NotFound, $"Child with id = {id} was not found.");
            }
        }
    }
}