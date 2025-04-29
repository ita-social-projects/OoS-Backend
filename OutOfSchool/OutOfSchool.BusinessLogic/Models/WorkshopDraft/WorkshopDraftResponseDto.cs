using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResponseDto
{
    public Guid WorkshopDraftId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }      
    public string? RejectionMessage { get; set; }
    public WorkshopV2Dto WorkshopDetails { get; set; }
}

public static class WorkshopDraftResponseDtoExtensions
{
    public static WorkshopDraftResponseDto ToResponseDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)
        => new()
        {
            WorkshopDraftId = draft.Id,
            DraftStatus = draft.DraftStatus,
            RejectionMessage = draft.RejectionMessage,
            WorkshopDetails = draft.ToDto()
        };

    public static List<WorkshopDraftResponseDto> ToResponseDto(this IEnumerable<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> list)
        => list.MapToList(ToResponseDto);
}