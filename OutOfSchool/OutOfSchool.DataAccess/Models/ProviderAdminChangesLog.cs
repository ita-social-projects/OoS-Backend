using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class ProviderAdminChangesLog : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required]
    public string ProviderAdminUserId { get; set; }

    public virtual User ProviderAdminUser { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public virtual Provider Provider { get; set; }

    public Guid? ManagedWorkshopId { get; set; }

    public virtual Workshop ManagedWorkshop { get; set; }

    [Required]
    public OperationType OperationType { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime OperationDate { get; set; }

    [Required]
    public string UserId { get; set; }

    public virtual User User { get; set; }
}