using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Changes;

public class ProviderAdminChangesLogRequest : SearchStringFilter
{
    public ProviderAdminType AdminType { get; set; }

    public OperationType? OperationType { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}