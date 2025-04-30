
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Official
{
    public class PromoteToDirectorResponseDto
    {
        public Guid OfficialId { get; set; }
        public Guid PositionId { get; set; }
        public DateOnly ActiveFrom { get; set; }
        public PositionType PositionType { get; set; }
        public string FullName { get; set; }
    }
}

public static class OfficialExtensions
{
    public static PromoteToDirectorResponseDto ToPromoteDto(
        this OutOfSchool.Services.Models.Official official,
        OutOfSchool.BusinessLogic.Models.Position.PositionDto newPosition)
    {
        return new PromoteToDirectorResponseDto
        {
            OfficialId = official.Id,
            PositionId = newPosition.Id,
            ActiveFrom = newPosition.ActiveFrom,
            PositionType = newPosition.PositionType,
            FullName = newPosition.FullName,
        };
    }
}
