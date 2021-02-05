using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services.Implementations
{
    public class SectionService : ISectionService
    {
        private readonly IMapper _mapper;
        private OutOfSchoolDbContext _context;

        public SectionService(IMapper mapper, OutOfSchoolDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Section> CreateAsync(SectionDto sectionDto)
        {
            if (sectionDto == null)
            {
                throw new ArgumentNullException($"{nameof(CreateAsync)} entity must not be null");
            }

            try
            {
                var section = _mapper.Map<SectionDto, Section>(sectionDto);

                await _context.Sections.AddAsync(section);
                await _context.SaveChangesAsync();

                return section;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(CreateAsync)} could not be saved: {ex.Message}");
            }
        }
    }
}