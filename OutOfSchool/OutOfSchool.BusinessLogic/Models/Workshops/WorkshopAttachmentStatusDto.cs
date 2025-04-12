using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopAttachmentStatusDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; }

    [Required]
    public bool IsAttached { get; set; }
}
