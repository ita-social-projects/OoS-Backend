using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Models.Application;

public class ApplicationCreate
{
    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public Guid ChildId { get; set; }

    public Guid ParentId { get; set; }
}

public static class ApplicationCreateExtensions
{
    public static OutOfSchool.Services.Models.Application ToModel(this ApplicationCreate application)
        => new()
        {
            WorkshopId = application.WorkshopId,
            ChildId = application.ChildId,
            ParentId = application.ParentId,
        };
}