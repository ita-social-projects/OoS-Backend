using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Validators;

namespace OutOfSchool.WebApi.Models.Parent;

public class ParentBlockByAdminDto
{
    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public bool ToggleBlock { get; set; }

    [MaxLength(500)]
    [RequiredIf("ToggleBlock", true, ErrorMessage = "Reason is required")]
    public string Reason { get; set; }
}
