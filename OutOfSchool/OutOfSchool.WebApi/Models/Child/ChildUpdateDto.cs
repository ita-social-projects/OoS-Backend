namespace OutOfSchool.WebApi.Models;

public class ChildUpdateDto : ChildBaseDto
{
    public List<long> SocialGroupIds { get; set; }
}
