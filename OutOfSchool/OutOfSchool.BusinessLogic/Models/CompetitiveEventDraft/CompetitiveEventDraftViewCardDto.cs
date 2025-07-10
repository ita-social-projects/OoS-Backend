using OutOfSchool.Services.Enums.CompetitiveEventStatus;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftViewCardDto
{
    public Guid CompetitiveEventDraftId { get; set; }
    public CompetitiveEventDraftStatus DraftStatus { get; set; }
    public string RejectionMessage { get; set; }
    public string Title { get; set; }
    public string CoverImageId { get; set; }
}

public static class CompetitiveEventDraftViewCardDtoExtensions
{
    public static CompetitiveEventDraftViewCardDto ToCardDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
        => new()
        {
            CompetitiveEventDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,
            Title = draft.CompetitiveEventDraftContent?.Title,
            CoverImageId = draft.CoverImageId,
        };

    public static List<CompetitiveEventDraftViewCardDto> ToCardDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft> list) => list.MapToList(ToCardDto);
}
