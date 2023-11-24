namespace OutOfSchool.WebApi.Models;

public class ChildCreateDto : ChildBaseDto
{
    public List<long> SocialGroupIds { get; set; }
}
