﻿using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Mapping.Extensions;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for ParentController
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly IEntityRepository<Parent> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        public ParentService(IEntityRepository<Parent> entityRepository)
        {
            this.repository = entityRepository;
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Create(ParentDTO parent)
        {
            CreateValidation(parent);
            Parent res = await repository.Create(parent.ToDomain()).ConfigureAwait(false);
            return res.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            Parent parent = await repository.GetById(id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException("This id doesn't exist", nameof(id));
            }

            await repository.Delete(parent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ParentDTO>> GetAll()
        {
            IEnumerable<Parent> parents = await this.repository.GetAll().ConfigureAwait(false);
            return parents.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetById(long id)
        {
            Parent parent = await repository.GetById((int)id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException("Not Found", nameof(id));
            }

            return parent.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Update(ParentDTO parent)
        {
                await UpdateValidation(parent);
                Parent res = await repository.Update(parent.ToDomain());
                return res.ToModel();
        }

        private async Task UpdateValidation(ParentDTO parent)
        {
            if (parent == null)
            {
                throw new ArgumentException(nameof(parent), "Parent is null");
            }

            if (parent.FirstName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty firstname.");
            }

            if (parent.LastName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty lastname.");
            }

            if (parent.MiddleName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty middlename.");
            }

            Parent tmp = await repository.GetById((int)parent.Id).ConfigureAwait(false);
            if (tmp == null)
            {
                throw new ArgumentException(nameof(parent), "Wrong id");
            }
        }

        public void CreateValidation(ParentDTO parent)
        {
            if (parent == null)
            {
                throw new ArgumentException(nameof(parent), "Parent is null");
            }

            if (parent.FirstName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty firstname.");
            }

            if (parent.LastName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty lastname.");
            }

            if (parent.MiddleName.Length == 0)
            {
                throw new ArgumentException(nameof(parent), "Empty middlename.");
            }
        }
    }
}
