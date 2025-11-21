namespace OutOfSchool.BusinessLogic.Models.Department;

public class DepartmentFilter : SearchStringFilter
{
    public string? FilterByProperty { get; set; }
    public bool Order { get; set; } = true;
}

