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
            CreateMap<Workshop, WorkshopDTO>().ReverseMap();
        }
    }
}