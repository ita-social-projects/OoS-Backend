using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class BlockedProviderParent : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; }

    [Required]
    public string UserIdBlock { get; set; }

    public string UserIdUnblock { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset DateTimeFrom { get; set; }

    public DateTimeOffset? DateTimeTo { get; set; }

    public virtual Parent Parent { get; set; }

    public virtual Provider Provider { get; set; }

}