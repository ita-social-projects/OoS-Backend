using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.AddressDraft;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftCreateDto : WorkshopDraftBaseDto
{
    [Required]
    public Guid ProviderId { get; set; }
    public List<long> TagsIds { get; set; }
    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public AddressDraftBaseDto Address { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<IFormFile> ImageFiles { get; set; }

    [RequireDefaultTeacherAttribute]    
    public List<TeacherDraftCreateDto> Teachers { get; set; }
}
