using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDraftViewCardDto
{
    public Guid WorkshopDraftId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }
    public string Title { get; set; }
}
