using OutOfSchool.WebApi.Models.SocialGroup;

namespace OutOfSchool.WebApi.Models;

public class ChildDto : ChildBaseDto
{
    public Guid Id { get; set; }

    public ParentDtoWithContactInfo Parent{ get; set; }

    public List<SocialGroupDto> SocialGroups { get; set; }
}