using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.BusinessLogic.Models.Achievement;

namespace OutOfSchool.BusinessLogic.Models;

public class AchievementDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(2000)]
    [MinLength(1)]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Column(TypeName = "date")]
    public DateTime AchievementDate { get; set; } = default;

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public long AchievementTypeId { get; set; }

    public List<ChildDto> Children { get; set; }

    public List<AchievementTeacherDto> Teachers { get; set; }
}

public static class AchievementDtoExtensions
{
    public static OutOfSchool.Services.Models.Achievement ToModel(this AchievementDto achievement)
        => new()
        {
            Id = achievement.Id,
            Title = achievement.Title,
            AchievementDate = achievement.AchievementDate,
            WorkshopId = achievement.WorkshopId,
            AchievementTypeId = achievement.AchievementTypeId,
        };

    public static AchievementDto ToDto(this OutOfSchool.Services.Models.Achievement achievement)
        => new()
        {
            Id = achievement.Id,
            Title = achievement.Title,
            AchievementDate = achievement.AchievementDate,
            WorkshopId = achievement.WorkshopId,
            AchievementTypeId = achievement.AchievementTypeId,
            Children = achievement.Children?.ToNotDeletedDto() ?? [],
            Teachers = achievement.Teachers?.ToNotDeletedDto() ?? []
        };

    public static List<AchievementDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Achievement> list)
        => list.MapToList(ToDto); 
}