#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class Official : BusinessEntity
{
    [Required]
    public Position Position { get; set; } = null!;

    [Required]
    public Individual Individual { get; set; } = null!;

    // TODO: type should be Document?
    public string? DismissalOrder { get; set; }

    // TODO: type should be Document?
    public string? RecruitmentOrder { get; set; }

    public string? DismissalReason { get; set; }

    // TODO: type should be EmployemntType enum?
    public string? EmploymentType { get; set; }

    // TODO: will be retrieved from aikom
    public Guid ExternalRegistryId { get; set; } = default;
}