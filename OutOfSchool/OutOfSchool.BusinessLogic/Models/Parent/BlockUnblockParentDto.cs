using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Validators;

namespace OutOfSchool.BusinessLogic.Models.Parent;

public class BlockUnblockParentDto
{
    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public bool IsBlocked { get; set; }

    [MaxLength(500)]
    [RequiredIf("IsBlocked", true, ErrorMessage = "Reason is required")]
    public string Reason { get; set; }
}
