using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Workshops.V2;

public class WorkshopV2CreateRequestDto : WorkshopCreateRequestDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}