using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Mapping
{
    public class SocialGroupMapperProfile :Profile
    {
        public SocialGroupMapperProfile()
        {
            CreateMap<SocialGroup, SocialGroupDTO>().ForMember(c => c.Id, d => d.MapFrom(socialGroup => socialGroup.SocialGroupId))
                .ForMember(c => c.ChildrenIds, d => d.MapFrom(socialGroup => socialGroup.Children.Select(x => x.ChildId))).ReverseMap();
        }
    }
}
