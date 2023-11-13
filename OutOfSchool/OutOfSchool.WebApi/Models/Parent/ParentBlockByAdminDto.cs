using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Parent;

public class ParentBlockByAdminDto
{
    [Required]
    public Guid ParentId { get; set; }

    [Required]
    public bool IsBlocked { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; }
}
