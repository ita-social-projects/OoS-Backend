using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

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
        public async Task<ChildDTO> Create(ChildDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Child was null.");
            }

            if (dto.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Invalid Date of birth");
            }

            try
            {
                var child = dto.ToDomain();

                var newChild = await repository.Create(child).ConfigureAwait(false);

                return newChild.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(dto)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            try
            {
                var children = await repository.GetAll().ConfigureAwait(false);

                return children.Select(child => child.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Children: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            var child = await repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Child was null.");
            }

            if (dto.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Wrong date of birth.", nameof(dto));
            }

            if (dto.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(dto));
            }

            if (dto.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(dto));
            }

            if (dto.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(dto));
            }

            try
            {
                var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return child.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(dto)} could not be updated: {ex.Message}");
            }
        }

        /// <inheritdoc/>
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