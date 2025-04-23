namespace OutOfSchool.BusinessLogic.Models;

public class ChildCreateDto : ChildBaseDto
{
    public List<long> SocialGroupIds { get; set; }
}

public static class ChildCreateDtoExtensions
{
    public static Child ToModel(this ChildCreateDto dto, Guid parentId)
        => new()
        {
            Id = Guid.Empty,
            ParentId = parentId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            PlaceOfStudy = dto.PlaceOfStudy,
            IsParent = dto.IsParent,    
            SocialGroups = []
        };
}
