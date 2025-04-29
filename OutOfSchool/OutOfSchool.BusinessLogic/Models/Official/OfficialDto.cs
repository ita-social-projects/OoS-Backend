using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Official;

public class OfficialDto
{
    public Guid Id { get; set; }
    public Guid PositionId { get; set; }
    public string Position { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Rnokpp { get; set; }
    public string DismissalOrder { get; set; } = string.Empty;
    public string RecruitmentOrder { get; set; } = string.Empty;
    public string DismissalReason { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}

public static class OfficialDtoExtensions
{
    public static OfficialDto ToDto(this OutOfSchool.Services.Models.Official model)
        => new()
        {
            Id = model.Id,
            PositionId = model.Position?.Id ?? default,
            Position = model.Position?.FullName,
            FirstName = model.Individual?.FirstName,
            MiddleName = model.Individual?.MiddleName,
            LastName = model.Individual?.LastName,
            Rnokpp = model.Individual?.Rnokpp,
            DismissalOrder = model.DismissalOrder ?? string.Empty,
            RecruitmentOrder = model.RecruitmentOrder ?? string.Empty,
            DismissalReason = model.DismissalReason ?? string.Empty,
            EmploymentType = model.EmploymentType,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
        };

    public static List<OfficialDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Official> list)
        => list.MapToList(ToDto);
}