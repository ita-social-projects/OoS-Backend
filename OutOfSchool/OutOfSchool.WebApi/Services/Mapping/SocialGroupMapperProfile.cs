using AutoMapper;
using OutOfSchool.Services.Models;
using System.Linq;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services.Mapping
{
    /// <summary>
    /// SocialGroup Mapper. Map SocialGroup to SocialGroupDTO.
    /// </summary>
    public class SocialGroupMapperProfile :Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialGroupMapperProfile"/> class.
        /// </summary>
        public SocialGroupMapperProfile()
        {
            this.CreateMap<SocialGroup, SocialGroupDTO>().ForMember(c => c.Id, d => d.MapFrom(socialGroup => socialGroup.SocialGroupId))
                .ForMember(c => c.ChildrenIds, d => d.MapFrom(socialGroup => socialGroup.Children.Select(x => x.ChildId))).ReverseMap();
        }
    }
}
