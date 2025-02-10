using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Util.Mapping;

public class ContactsProfile : Profile
{
    public ContactsProfile()
    {
        CreateMap<SocialNetworkDto, SocialNetwork>()
            .ReverseMap();
        CreateMap<EmailDto, Email>()
            .ReverseMap();
        CreateMap<PhoneNumberDto, PhoneNumber>()
            .ReverseMap();
        CreateMap<ContactsAddressDto, ContactsAddress>()
            .ForMember(dest => dest.CATOTTG, opt => opt.Ignore())
            .ForMember(dest => dest.GeoHash, opt => opt.Ignore());
        CreateMap<ContactsAddress, ContactsAddressDto>()
            .ForMember(dest => dest.CodeficatorAddressDto, opt => opt.MapFrom(src => src.CATOTTG));
        // Reverse mapping exists for explicit mapping in service, should be ignored on general entity
        CreateMap<Contacts, ContactsDto>()
            .ReverseMap();
    }
}