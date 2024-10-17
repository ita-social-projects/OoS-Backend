using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.ProvidersInfo;

public class ProviderSectionItemInfoDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}
