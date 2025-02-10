using Newtonsoft.Json;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
public class TeacherDraftCreateDto : TeacherDraftDto
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }
}
