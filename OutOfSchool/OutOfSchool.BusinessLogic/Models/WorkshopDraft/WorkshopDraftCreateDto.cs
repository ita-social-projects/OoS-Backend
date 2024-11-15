namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
 public class WorkshopDraftCreateDto
{
    public Guid ProviderId { get; set; }

    public WorkshopDraftContentDto WorkshopDraftContent { get; set; }

    public List<long> TagsIds { get; set; }

    public IFormFile CoverImage { get; set; }

    public List<IFormFile> ImageFiles { get; set; }
}
