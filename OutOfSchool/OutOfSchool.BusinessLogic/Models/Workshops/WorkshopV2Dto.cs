using Newtonsoft.Json;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopV2Dto : WorkshopDto, IHasCoverImage, IHasImages
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<IFormFile> ImageFiles { get; set; }
}
