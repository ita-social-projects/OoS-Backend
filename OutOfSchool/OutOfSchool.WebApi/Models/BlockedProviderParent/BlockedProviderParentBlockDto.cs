using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.BlockedProviderParent;

public class BlockedProviderParentBlockDto
{
    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; }
}