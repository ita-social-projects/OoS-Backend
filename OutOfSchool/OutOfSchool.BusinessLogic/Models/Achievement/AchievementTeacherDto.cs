using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Achievement;

public class AchievementTeacherDto
{
    public long Id { get; set; }

    [Required]
    public Guid AchievementId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }
}

public static class AchievementTeacherDtoExtensions
{
    public static AchievementTeacher ToModel(this AchievementTeacherDto teacher)
        => new()
        {
            Id = teacher.Id,
            AchievementId = teacher.AchievementId,
            Title = teacher.Title,            
        };

    public static AchievementTeacherDto ToDto(this AchievementTeacher teacher)
        => new()
        {
            Id = teacher.Id,
            AchievementId = teacher.AchievementId,
            Title = teacher.Title,
        };

    public static List<AchievementTeacherDto> ToNotDeletedDto(this IEnumerable<AchievementTeacher> list)
        => list.MapNonDeletedToList(ToDto);
}