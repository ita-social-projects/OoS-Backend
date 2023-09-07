using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopDescriptionItemDTO
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid WorkshopId { get; set; }
}