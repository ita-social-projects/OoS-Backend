using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopV2CreateRequestDto : WorkshopCreateRequestDto
{
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}