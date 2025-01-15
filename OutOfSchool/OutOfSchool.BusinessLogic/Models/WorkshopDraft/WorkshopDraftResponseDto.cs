using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.AddressDraft;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResponseDto : WorkshopDraftBaseDto
{
    public Guid Id { get; set; }
    public string CoverImageId { get; set; }
    public Guid ProviderId { get; set; }
    public WorkshopDraftStatus DraftStatus { get; set; }
    public List<string> ImagesIds { get; set; }
    public List<TagDto> Tags { get; set; }
    public AddressDraftDto Address {  get; set; }
}
