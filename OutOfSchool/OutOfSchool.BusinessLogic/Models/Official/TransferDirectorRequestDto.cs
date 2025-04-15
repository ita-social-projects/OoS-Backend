
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Official;
public class TransferDirectorRequestDto
{
    [Required]
    public Guid FromOfficialId { get; set; }

    [Required]
    public Guid ToOfficialId { get; set; }
}
