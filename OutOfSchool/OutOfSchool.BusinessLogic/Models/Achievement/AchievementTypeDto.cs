using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class AchievementTypeDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(200)]
    [MinLength(1)]
    public string Title { get; set; }
}

public static class AchievementTypeDtoExtensions
{
    public static AchievementTypeDto ToDto(this AchievementType achievementType)
        => new()
        {
            Id = achievementType.Id,
            Title = achievementType.Title,
        };

    public static List<AchievementTypeDto> ToDto(this IEnumerable<AchievementType> list)
        => list.MapToList(ToDto);
}