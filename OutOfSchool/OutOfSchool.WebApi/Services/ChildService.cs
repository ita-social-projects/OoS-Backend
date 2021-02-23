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
    /// Service with business logic for Child model.
    /// </summary>
    public class ChildService : IChildService
    {
        private IEntityRepository<Child> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for the Child entity.</param>
        public ChildService(IEntityRepository<Child> entityRepository)
        {
            _repository = entityRepository;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO childDTo)
        {
            if (childDTo == null)
            {
                throw new ArgumentNullException(nameof(childDTo), "Child was null.");
            }

            if (childDTo.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Invalid Date of birth");
            }

            try
            {
                var child = childDTo.ToDomain();

                var newChild = await _repository.Create(child).ConfigureAwait(false);

                return newChild.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(childDTo)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            try
            {
                var children = await _repository.GetAll().ConfigureAwait(false);
               
                var childrenDto = children.Select(child => child.ToModel());

                return childrenDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve Children: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            var child = await _repository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO childDto)
        {
            if (childDto == null)
            {
                throw new ArgumentNullException(nameof(childDto), "Child was null.");
            }

            if (childDto.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Wrong date of birth.", nameof(childDto));
            }

            if (childDto.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(childDto));
            }

            if (childDto.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(childDto));
            }

            if (childDto.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(childDto));
            }

            try
            {
                var child = await _repository.Update(childDto.ToDomain()).ConfigureAwait(false);
              
                return child.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(childDto)} could not be updated: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            ChildDTO childDto;

            try
            {
                childDto = await GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            await _repository
                .Delete(childDto.ToDomain())
                .ConfigureAwait(false);
        }
    }
}