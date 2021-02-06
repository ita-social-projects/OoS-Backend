using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Mapping
{
    public class ChildMapperProfile : Profile
    {
        public ChildMapperProfile()
        {
            CreateMap<Child, ChildDTO>().ForMember(c => c.Id, d => d.MapFrom(child => child.ChildId)).ReverseMap();
        }
    }
}
