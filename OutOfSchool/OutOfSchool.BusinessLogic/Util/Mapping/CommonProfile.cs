using OutOfSchool.Common.Models;
using Profile = AutoMapper.Profile;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public class CommonProfile : Profile
{
    public CommonProfile()
    {
        CreateMap<object, IHasRating>()
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfRatings, opt => opt.Ignore());
    }
}