using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;
public class OperationWithObject : IKeyedEntity<Guid> {
    public Guid Id { get; set; }

    public Guid? EntityId { get; set; }

    public OperationWithObjectEntityType? EntityType { get; set; }

    [Required]
    public OperationWithObjectOperationType OperationType { get; set; }

    public string RowSeparator { get; set; }

    [Required]
    public DateTimeOffset EventDateTime { get; set; } = DateTimeOffset.Now;

    public string Comment { get; set; }
}
