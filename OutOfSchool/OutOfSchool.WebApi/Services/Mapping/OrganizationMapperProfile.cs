using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Mapping
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
            this.CreateMap<Organization, OrganizationDTO>().ForMember(c => c.Id, d => d.MapFrom(organization => organization.Id)).ReverseMap();
        }
    }
}
