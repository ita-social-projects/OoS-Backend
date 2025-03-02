using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResponseDto : WorkshopDraftBaseDto
{
    public Guid WorkshopDraftId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }      
    public string? RejectionMessage { get; set; }
    public WorkshopV2Dto WorkshopDetails { get; set; }
}
