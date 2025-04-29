using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDraftViewCardDto
{
    public Guid WorkshopDraftId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }
    public string? RejectionMessage { get; set; }
    public string Title { get; set; }

    public string CoverImageId { get; set; }
    public List<long> DirectionIds { get; set; }
}

public static class WorkshopDraftViewCardDtoExtensions
{
    public static WorkshopDraftViewCardDto ToCardDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)
        => new()
        {
            WorkshopDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,
            Title = draft.WorkshopDraftContent?.Title,
            CoverImageId = draft.CoverImageId,
        };

    public static List<WorkshopDraftViewCardDto> ToCardDto(this IEnumerable<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> list)
        => list.MapToList(ToCardDto);
}
