using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Models;

public class ParentCard : WorkshopCard
{
    [Required]
    public Guid ChildId { get; set; }

    [Required]
    public Guid ApplicationId { get; set; }

    public ApplicationStatus Status { get; set; }
}