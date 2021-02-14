using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Mapping.Sections
{
    public class SectionProfile : Profile
    {
        public SectionProfile()
        {
            CreateMap<SectionDTO, Section>();
        }
    }
}