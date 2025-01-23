using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.ProvidersGeneral;
public class ProviderGeneralCreateUpdateDto : ProviderGeneralBaseDto
{
    [Required]
    public Guid ContactId { get; set; } 
    [Required]
    public long ClassifierTypeId { get; set; }

    [Required]
    public override IList<string> ImageIds { get; set; } = new List<string>();

    [JsonIgnore]
    public override List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
}
