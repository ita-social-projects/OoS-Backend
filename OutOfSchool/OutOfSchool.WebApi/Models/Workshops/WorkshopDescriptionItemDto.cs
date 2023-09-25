using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Workshops;

public class WorkshopDescriptionItemDto
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