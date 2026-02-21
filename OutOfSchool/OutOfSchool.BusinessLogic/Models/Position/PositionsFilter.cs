namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionsFilter : SearchStringFilter
{   
    public string? FilterByProperty { get; set; }
    public bool Order { get; set; } = true;
}