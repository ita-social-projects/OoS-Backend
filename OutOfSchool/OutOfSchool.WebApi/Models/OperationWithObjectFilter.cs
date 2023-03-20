using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class OperationWithObjectFilter
{
    public Guid? EntityId { get; set; } = null;

    public OperationWithObjectEntityType? EntityType { get; set; } = null;

    public OperationWithObjectOperationType OperationType { get; set; }

    public string RowSeparator { get; set; } = string.Empty;

    public DateTimeOffset? EventDateTime { get; set; } = null;
}
