using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopV2Dto : WorkshopDto, IHasCoverImage, IHasImages
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}
