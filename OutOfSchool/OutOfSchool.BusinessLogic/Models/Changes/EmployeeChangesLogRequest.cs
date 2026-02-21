using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class EmployeeChangesLogRequest : SearchStringFilter
{
    public OperationType? OperationType { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}