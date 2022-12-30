using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Application;

public class ApplicationDto
{
    public Guid Id { get; set; }

    [EnumDataType(typeof(ApplicationStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [MaxLength(500)]
    public string RejectionMessage { get; set; }

    public DateTimeOffset CreationTime { get; set; }

    public DateTimeOffset? ApprovedTime { get; set; }

    public bool IsBlocked { get; set; }

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public Guid ChildId { get; set; }

    public Guid ParentId { get; set; }

    public WorkshopCard Workshop { get; set; }

    public ChildDto Child { get; set; }

    public ParentDTO Parent { get; set; }
}