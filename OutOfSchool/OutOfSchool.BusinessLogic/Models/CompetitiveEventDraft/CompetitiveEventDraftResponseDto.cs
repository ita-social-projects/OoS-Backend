using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;


namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftResponseDto
{
    public Guid CompetitiveEventDraftId { get; set; }
    public CompetitiveEventDraftStatus DraftStatus { get; set; }
    public string? RejectionMessage { get; set; }
    public CompetitiveEventV2Dto CompetitiveEventDetails { get; set; }
}

public static class CompetitiveEventDraftResponseDtoExtensions
{
    public static CompetitiveEventDraftResponseDto ToResponseDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
        => new()
        {
            CompetitiveEventDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,
            CompetitiveEventDetails = draft.ToDto()
        };

    public static List<CompetitiveEventDraftResponseDto> ToResponseDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft> list) => list.MapToList(ToResponseDto);
}