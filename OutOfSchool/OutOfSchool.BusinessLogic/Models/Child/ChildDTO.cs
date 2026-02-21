using OutOfSchool.BusinessLogic.Models.SocialGroup;

namespace OutOfSchool.BusinessLogic.Models;

public class ChildDto : ChildBaseDto
{
    public Guid Id { get; set; }

    public Guid ParentId { get; set; } = Guid.Empty;

    public ParentDtoWithContactInfo Parent{ get; set; }

    public List<SocialGroupDto> SocialGroups { get; set; }
}

public static class ChildDtoExtensions
{
    public static Child ToModel(this ChildDto child)
        => new()
        {
            Id = child.Id,
            DateOfBirth = child.DateOfBirth,
            FirstName = child.FirstName,
            Gender = child.Gender,
            IsParent = child.IsParent,
            LastName = child.LastName,
            MiddleName = child.MiddleName ?? string.Empty,
            ParentId = child.ParentId,
            PlaceOfStudy = child.PlaceOfStudy,            
        };

    public static ChildDto ToDto(this Child child)
        => new()
        {
            Id = child.Id,
            DateOfBirth = child.DateOfBirth,
            FirstName = child.FirstName,
            Gender = child.Gender,
            IsParent = child.IsParent,
            LastName = child.LastName,
            MiddleName = child.MiddleName ?? string.Empty,
            Parent = child.Parent?.ToContactInfoDto(),
            ParentId = child.ParentId,
            PlaceOfStudy = child.PlaceOfStudy,
            SocialGroups = child.SocialGroups?.ToNotDeletedDto()
        };

    public static List<ChildDto> ToDto(this IEnumerable<Child> list)
        => list.MapToList(ToDto);

    public static List<ChildDto> ToNotDeletedDto(this IEnumerable<Child> list)
        => list.MapNonDeletedToList(ToDto);
}