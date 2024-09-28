using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class EmployeeChangesLog : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required]
    public string EmployeeUserId { get; set; }

    public virtual User EmployeeUser { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public virtual Provider Provider { get; set; }

    [Required]
    public OperationType OperationType { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime OperationDate { get; set; }

    [Required]
    public string UserId { get; set; }

    public virtual User User { get; set; }

    [Required]
    [MaxLength(128)]
    public string PropertyName { get; set; }

    [MaxLength(500)]
    public string OldValue { get; set; }

    [MaxLength(500)]
    public string NewValue { get; set; }
}