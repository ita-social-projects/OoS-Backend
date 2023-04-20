using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.BlockedProviderParent;

public class BlockedProviderParentUnblockDto
{
    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public Guid ProviderId { get; set; }
}