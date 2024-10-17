using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDescriptionItemDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid CompetitiveEventId { get; set; }
}
