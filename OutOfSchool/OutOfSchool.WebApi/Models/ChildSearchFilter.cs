namespace OutOfSchool.WebApi.Models;

public class ChildSearchFilter : SearchStringFilter
{
    public bool? IsParent { get; set; } = null;
}
