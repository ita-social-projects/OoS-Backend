using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ChangesLogFilter : ChangesLogFilterBase
{
    [Required]
    public string EntityType { get; set; }
}