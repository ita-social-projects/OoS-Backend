using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping
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
            this.CreateMap<SocialGroup, SocialGroupDTO>().ForMember(c => c.Id, d => d.MapFrom(socialGroup => socialGroup.Id))
                .ForMember(c => c.ChildrenIds, d => d.MapFrom(socialGroup => socialGroup.Children.Select(x => x.Id))).ReverseMap();
        }
    }
}
