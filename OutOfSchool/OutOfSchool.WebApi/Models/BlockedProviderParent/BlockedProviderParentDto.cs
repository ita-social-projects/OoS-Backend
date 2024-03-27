using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.BlockedProviderParent;

public class BlockedProviderParentDto
{
    public Guid Id { get; set; }

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
}