using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services.Implementation
{
    public class SectionService : ISectionService
    {
        private IEntityRepository<Section> SectionRepository { get; set; }
        private readonly IMapper mapper;

        public SectionService(IMapper mapper, IEntityRepository<Section> sectionRepository)
        {
            this.mapper = mapper;
            this.SectionRepository = sectionRepository;
        }

        /// <inheritdoc/>
        public async Task<SectionDTO> Create(SectionDTO section)
        {
            if (section == null)
            {
                throw new ArgumentNullException($"{nameof(SectionDTO)} entity must not be null");
            }

            try
            {
                var newSection = mapper.Map<SectionDTO, Section>(section);

                var section_ = await SectionRepository.Create(newSection).ConfigureAwait(false);

                return section;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(SectionDTO)} could not be saved: {ex.Message}");
            }
        }

        public IEnumerable<SectionDTO> GetAll()
        {
            var sectionDTO = this.SectionRepository.GetAll().Select(
                x => this.mapper.Map<Section, SectionDTO>(x));

            return sectionDTO;
        }
    }
}