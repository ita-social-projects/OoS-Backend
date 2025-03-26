using OutOfSchool.BusinessLogic.Models.SocialGroup;

namespace OutOfSchool.BusinessLogic.Models;

public class ChildDto : ChildBaseDto
{
    public Guid Id { get; set; }

    public Guid ParentId { get; set; } = Guid.Empty;

    public ParentDtoWithContactInfo Parent{ get; set; }

    public List<SocialGroupDto> SocialGroups { get; set; }
}