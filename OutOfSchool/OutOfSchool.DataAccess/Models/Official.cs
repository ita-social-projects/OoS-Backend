#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Official : BusinessEntity
{
    [Required]
    public Guid PositionId { get; set; }

    public virtual Position Position { get; set; } = null!;
    
    [Required]
    public Guid IndividualId { get; set; }

    public virtual Individual Individual { get; set; } = null!;

    // TODO: type should be Document?
    [MaxLength(2000)]
    public string? DismissalOrder { get; set; }

    // TODO: type should be Document?
    [MaxLength(2000)]
    public string? RecruitmentOrder { get; set; }
    
    [MaxLength(255)]    
    public string? DismissalReason { get; set; }
    
    public EmploymentType EmploymentType { get; set; }

    // TODO: will be retrieved from aikom
    public Guid ExternalRegistryId { get; set; } = default;
}