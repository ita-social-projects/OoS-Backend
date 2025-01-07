namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionsFilter : SearchStringFilter
{
    public bool OrderByFullName { get; set; } = true;

    public bool OrderByCreatedAt { get; set; } = true;
}
