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
    ///  Service with business logic for Organization model.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private IOrganizationRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        public OrganizationService(IOrganizationRepository entityRepository)
        {
            _repository = entityRepository;
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Create(OrganizationDTO organizationDto)
        {
            if (organizationDto == null)
            {
                throw new ArgumentNullException(nameof(organizationDto), "Organization was null.");
            }

            var organization = organizationDto.ToDomain();

            if (!_repository.IsUnique(organization))
            {
                throw new ArgumentException(nameof(organization), "There is already an organizationDto with such data");
            }

            var newOrganization = await _repository.Create(organization).ConfigureAwait(false);

            return newOrganization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrganizationDTO>> GetAll()
        {
             var organizations = await _repository.GetAll().ConfigureAwait(false);
            
             return organizations.Select(organization => organization.ToModel());
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> GetById(long id)
        {
            var organization = await _repository.GetById(id).ConfigureAwait(false);
            if (organization == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return organization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Update(OrganizationDTO organizationDto)
        {
            if (organizationDto == null)
            {
                throw new ArgumentNullException(nameof(organizationDto), "Organization was null.");
            }

            if (organizationDto.EDRPOU.Length == 0)
            {
                throw new ArgumentException("EDRPOU code is empty", nameof(organizationDto));
            }

            if (organizationDto.INPP.Length == 0)
            {
                throw new ArgumentException("INPP code is empty", nameof(organizationDto));
            }

            if (organizationDto.MFO.Length == 0)
            {
                throw new ArgumentException("MFO code is empty", nameof(organizationDto));
            }

            if (organizationDto.Title.Length == 0)
            {
                throw new ArgumentException("Title is empty", nameof(organizationDto));
            }

            if (organizationDto.Description.Length == 0)
            {
                throw new ArgumentException("Description is empty", nameof(organizationDto));
            }

            try
            {
                var organization = await _repository.Update(organizationDto.ToDomain()).ConfigureAwait(false);
              
                return organization.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(organizationDto)} could not be updated: {ex.Message}");
            }
            
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            OrganizationDTO organizationDto;
            
            try
            {
                organizationDto = await GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            await _repository
                .Delete(organizationDto.ToDomain())
                .ConfigureAwait(false);
        }
    }
}