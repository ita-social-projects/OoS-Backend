namespace OutOfSchool.BusinessLogic.Models;

public class ChildSearchFilter : SearchStringFilter
{
    public bool? IsParent { get; set; } = null;
}
