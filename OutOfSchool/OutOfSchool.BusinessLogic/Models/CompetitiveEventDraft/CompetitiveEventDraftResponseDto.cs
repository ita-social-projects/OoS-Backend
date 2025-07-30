using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftResponseDto
{
    public Guid CompetitiveEventDraftId { get; set; }
    public CompetitiveEventDraftStatus DraftStatus { get; set; }
    public string? RejectionMessage { get; set; }
    public CompetitiveEventV2Dto CompetitiveEventDetails { get; set; }
    public string ProviderEdrpou { get; set; }
    public string DirectorFullName { get; set; }
    public string DirectorPosition { get; set; }
}

public static class CompetitiveEventDraftResponseDtoExtensions
{
    public static CompetitiveEventDraftResponseDto ToResponseDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
    {
        var dto = new CompetitiveEventDraftResponseDto()
        {
            CompetitiveEventDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,            
            CompetitiveEventDetails = draft.ToDto(),
            ProviderEdrpou = draft.Provider?.Edrpou ?? string.Empty,
            DirectorFullName = string.Empty,
            DirectorPosition = string.Empty,
        };

        var officials = draft.Provider?.Positions?.SelectMany(o => o.Officials).ToList();

        var director = officials?.FirstOrDefault(o => o.Position.PositionType == PositionType.Director);
        var deputy = officials?.FirstOrDefault(o => o.Position.PositionType == PositionType.DeputyDirector);

        if (director?.Individual is not null)
        {
            dto.DirectorFullName = $"{director.Individual.FirstName} {director.Individual.LastName} {director.Individual.MiddleName}".Trim();
            dto.DirectorPosition = nameof(PositionType.Director);
        }

        else if (deputy?.Individual is not null)
        {
            dto.DirectorFullName = $"{deputy.Individual.FirstName} {deputy.Individual.LastName} {deputy.Individual.MiddleName}".Trim();
            dto.DirectorPosition = nameof(PositionType.DeputyDirector);
        }

        return dto;
    }

    public static List<CompetitiveEventDraftResponseDto> ToResponseDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft> list) => list.MapToList(ToResponseDto);
}