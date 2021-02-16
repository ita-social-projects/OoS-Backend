using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Tools.Mapping
{
    /// <summary>
    /// Mapper of Workshop to WorkshopDto.
    /// </summary>
    public class WorkshopMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopMapperProfile"/> class.
        /// </summary>
        public WorkshopMapperProfile()
        {
            this.CreateMap<Workshop, WorkshopDTO>()
                .ForMember(sectionDto => sectionDto.Id, d => d.MapFrom(section => section.Id)).ReverseMap()
                .ForMember(sectionDto => sectionDto.Address, options => options.Ignore()).ReverseMap()
                .ForMember(sectionDto => sectionDto.Organization, options => options.Ignore()).ReverseMap()
                .ForMember(sectionDto => sectionDto.Teachers, options => options.Ignore()).ReverseMap()
                .ForMember(sectionDto => sectionDto.Subcategory, options => options.Ignore()).ReverseMap();
        }
    }
}