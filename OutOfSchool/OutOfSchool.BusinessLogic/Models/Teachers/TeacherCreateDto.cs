using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Teachers;

/// <summary>
/// As teacher logic will be completely re-written later - property re-use and inheritance is ok here.
/// </summary>
[JsonDerivedType(typeof(TeacherUpdateDto))]
public class TeacherCreateDto : TeacherBaseDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }
}