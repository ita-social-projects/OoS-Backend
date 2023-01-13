using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Changes;

public class ChangesLogFilter : ChangesLogFilterBase
{
    [Required]
    public string EntityType { get; set; }
    public Guid? InstitutionId { get; set; }
}