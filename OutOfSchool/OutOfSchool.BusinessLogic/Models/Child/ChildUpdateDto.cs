namespace OutOfSchool.BusinessLogic.Models;

public class ChildUpdateDto : ChildBaseDto
{
    public List<long> SocialGroupIds { get; set; }
}

public static class ChildUpdateDtoExtensions
{
    public static Child SetToModel(this ChildUpdateDto dto, Child child)
    {
        child.FirstName = dto.FirstName;
        child.LastName = dto.LastName;
        child.MiddleName = dto.MiddleName;
        child.DateOfBirth = dto.DateOfBirth;
        child.Gender = dto.Gender;
        child.PlaceOfStudy = dto.PlaceOfStudy;
        child.IsParent = dto.IsParent;

        return child;
    }
}
