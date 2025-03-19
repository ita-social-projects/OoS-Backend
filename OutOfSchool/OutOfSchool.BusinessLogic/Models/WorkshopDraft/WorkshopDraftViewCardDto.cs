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
