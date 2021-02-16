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
    /// Configuring mapper for ParentService.
    /// </summary>
    public class ParentMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParentMapperProfile"/> class.
        /// </summary>
        public ParentMapperProfile()
        {
            this.CreateMap<Parent, ParentDTO>().ForMember(c => c.Id, d => d.MapFrom(parent => parent.ParentId)).ReverseMap();
        }
    }
}
