﻿using System;
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
    /// Service with business logic for Child model.
    /// </summary>
    public class ChildService : IChildService
    {
        private IEntityRepository<Child> ChildRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for the Child entity.</param>
        /// <param name="mapper">Mapper.</param>
        public ChildService(IEntityRepository<Child> entityRepository, IMapper mapper)
        {
            ChildRepository = entityRepository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child), "Child was null.");
            }

            if (child.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Invalid Date of birth");
            }

            try
            {
                var newChild = mapper.Map<ChildDTO, Child>(child);

                var child_ = await ChildRepository.Create(newChild).ConfigureAwait(false);

                return mapper.Map<Child, ChildDTO>(child_);
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(child)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            try
            {
                var childrenDto = await Task.Run(() => ChildRepository.GetAll().Result.Select(
                    x => mapper.Map<Child, ChildDTO>(x))).ConfigureAwait(false);

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
            var child = await ChildRepository.GetById(id).ConfigureAwait(false);

            if (child == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return mapper.Map<Child, ChildDTO>(child);
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO childDTO)
        {
            if (childDTO == null)
            {
                throw new ArgumentNullException(nameof(childDTO), "Child was null.");
            }

            if (childDTO.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Wrong date of birth.", nameof(childDTO));
            }

            if (childDTO.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(childDTO));
            }

            if (childDTO.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(childDTO));
            }

            if (childDTO.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(childDTO));
            }

            try
            {
                return mapper.Map<Child, ChildDTO>(await ChildRepository
                    .Update(mapper.Map<ChildDTO, Child>(childDTO))
                    .ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(childDTO)} could not be updated: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            ChildDTO childDTO;

            try
            {
                childDTO = await GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            await ChildRepository
                .Delete(mapper.Map<ChildDTO, Child>(childDTO))
                .ConfigureAwait(false);
        }
    }
}