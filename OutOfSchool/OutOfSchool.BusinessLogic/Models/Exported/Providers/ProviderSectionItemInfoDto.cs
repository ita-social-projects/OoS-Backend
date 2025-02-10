using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Providers;

public class ProviderSectionItemInfoDto
{
    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}
