using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Mapping
{
    /// <summary>
    /// Child Mapper. Map Child to ChildDTO.
    /// </summary>
    public class ChildMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildMapperProfile"/> class.
        /// </summary>
        public ChildMapperProfile()
        {
            CreateMap<Child, ChildDTO>().ForMember(c => c.Id, d => d.MapFrom(child => child.ChildId)).ReverseMap();
        }
    }
}
