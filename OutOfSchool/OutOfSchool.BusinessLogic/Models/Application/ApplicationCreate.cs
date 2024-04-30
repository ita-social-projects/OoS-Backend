using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Application;

public class ApplicationCreate
{
    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public Guid ChildId { get; set; }

    public Guid ParentId { get; set; }
}