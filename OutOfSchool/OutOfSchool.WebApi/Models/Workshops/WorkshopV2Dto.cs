using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models.Workshops;

public class WorkshopV2Dto : WorkshopDto
{
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<IFormFile> ImageFiles { get; set; }
}
