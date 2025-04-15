
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Official;
public class PromoteToDirectorResponseDto
{
    public Guid OfficialId { get; set; }
    public Guid PositionId { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public PositionType PositionType { get; set; }
    public string FullName { get; set; }
}

