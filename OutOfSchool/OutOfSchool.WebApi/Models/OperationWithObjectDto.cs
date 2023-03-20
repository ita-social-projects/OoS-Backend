using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class OperationWithObjectDto
{
    public Guid Id { get; set; }

    public Guid? EntityId { get; set; }

    [EnumDataType(typeof(OperationWithObjectEntityType), ErrorMessage = Constants.EnumErrorMessage)]
    public OperationWithObjectEntityType? EntityType { get; set; }

    [Required]
    [EnumDataType(typeof(OperationWithObjectOperationType), ErrorMessage = Constants.EnumErrorMessage)]
    public OperationWithObjectOperationType OperationType { get; set; }

    public string RowSeparator { get; set; }

    [Required]
    public DateTimeOffset EventDateTime { get; set; } = DateTimeOffset.UtcNow;

    public string Comment { get; set; }
}
