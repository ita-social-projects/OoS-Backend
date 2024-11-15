using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResponseDto
{
    public Guid Id { get; set; }

    public string CoverImageId { get; set; }

    public List<string> ImageIds { get; set; }

    public WorkshopDraftContentDto WorkshopDraftContent { get; set; }

    public WorkshopDraftStatus DraftStatus { get; set; }

    public Guid ProviderId { get; set; }
}
