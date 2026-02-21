using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.BlockedProviderParent;

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
public static class BlockedProviderParentBlockDtoExtensions
{
    public static OutOfSchool.Services.Models.BlockedProviderParent ToModel(this BlockedProviderParentBlockDto dto)
        => new()
        {
            ParentId = dto.ParentId,
            ProviderId = dto.ProviderId,
            Reason = dto.Reason,
        };
}