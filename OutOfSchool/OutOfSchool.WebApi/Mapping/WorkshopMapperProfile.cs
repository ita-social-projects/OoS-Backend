using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping
{
    /// <summary>
    /// Mapper of Workshop to WorkshopDto.
    /// </summary>
    public class WorkshopMapperProfile : Profile
    {
        private readonly IMapper _mapper;
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopMapperProfile"/> class.
        /// </summary>
        public WorkshopMapperProfile()
        {
            CreateMap<Workshop, WorkshopDTO>().ReverseMap();
        }

        public WorkshopDTO ToModel(Workshop workshop)
        {
            return _mapper.Map<WorkshopDTO>(workshop);
        }
        
        public Workshop ToDomain(WorkshopDTO workshopDto)
        {
            return _mapper.Map<Workshop>(workshopDto);
        }
    }
}