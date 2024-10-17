namespace OutOfSchool.BusinessLogic.Models;

public class ChildUpdateDto : ChildBaseDto
{
    public List<long> SocialGroupIds { get; set; }
}
