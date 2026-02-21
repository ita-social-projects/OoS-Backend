using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Official;
public class PromoteToDirectorRequestDto
{
    [Required]
    public Guid OfficialId { get; set; }
}
