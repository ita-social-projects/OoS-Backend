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
    ///  Service with business logic for Organization model.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private IOrganizationRepository OrganizationRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="mapper">Mapper.</param>
        public OrganizationService(IOrganizationRepository entityRepository, IMapper mapper)
        {
            OrganizationRepository = entityRepository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Create(OrganizationDTO organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Organization was null.");
            }

            var newOrganization = mapper.Map<OrganizationDTO, Organization>(organization);

            if (!OrganizationRepository.IsUnique(newOrganization))
            {
                throw new ArgumentException(nameof(newOrganization), "There is already an organization with such data");
            }

            var organization_ = await OrganizationRepository.Create(newOrganization).ConfigureAwait(false);

            return await Task.FromResult(mapper.Map<Organization, OrganizationDTO>(organization_))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrganizationDTO>> GetAll()
        {
            return await Task.Run(() => OrganizationRepository.GetAll().Result.Select(
                x => mapper.Map<Organization, OrganizationDTO>(x))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> GetById(long id)
        {
            var organization = await OrganizationRepository.GetById(id).ConfigureAwait(false);
            if (organization == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return mapper.Map<Organization, OrganizationDTO>(organization);
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Update(OrganizationDTO organizationDTO)
        {
            if (organizationDTO == null)
            {
                throw new ArgumentNullException(nameof(organizationDTO), "Organization was null.");
            }

            if (organizationDTO.EDRPOU.Length == 0)
            {
                throw new ArgumentException("EDRPOU code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.INPP.Length == 0)
            {
                throw new ArgumentException("INPP code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.MFO.Length == 0)
            {
                throw new ArgumentException("MFO code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.Title.Length == 0)
            {
                throw new ArgumentException("Title is empty", nameof(organizationDTO));
            }

            if (organizationDTO.Description.Length == 0)
            {
                throw new ArgumentException("Description is empty", nameof(organizationDTO));
            }

            try
            {
                return mapper.Map<Organization, OrganizationDTO>(await OrganizationRepository
                    .Update(mapper.Map<OrganizationDTO, Organization>(organizationDTO))
                    .ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(organizationDTO)} could not be updated: {ex.Message}");
            }
            
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            OrganizationDTO organizationDTO;
            
            try
            {
                organizationDTO = await GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            await OrganizationRepository
                .Delete(mapper.Map<OrganizationDTO, Organization>(organizationDTO))
                .ConfigureAwait(false);
        }
    }
}