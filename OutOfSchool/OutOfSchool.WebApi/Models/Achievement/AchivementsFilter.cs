namespace OutOfSchool.WebApi.Models.Achievement;

public class AchievementsFilter : SearchStringFilter
{
    public Guid WorkshopId { get; set; } = Guid.Empty;
}
