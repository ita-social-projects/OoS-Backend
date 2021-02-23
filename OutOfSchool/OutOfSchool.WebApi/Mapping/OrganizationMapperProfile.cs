using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping
{
    /// <summary>
    /// Organization Mapper. Map Organization to OrganizationDTO.
    /// </summary>
    public class OrganizationMapperProfile : Profile
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="OrganizationMapperProfile"/> class.
        /// </summary>
        public OrganizationMapperProfile()
        {
            CreateMap<Organization, OrganizationDTO>().ReverseMap();
        }
    }
}
