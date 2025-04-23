using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Application;

public class ApplicationDto
{
    public Guid Id { get; set; }

    [EnumDataType(typeof(ApplicationStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [MaxLength(500)]
    public string RejectionMessage { get; set; }

    public DateTimeOffset CreationTime { get; set; }

    public DateTimeOffset? ApprovedTime { get; set; }

    public bool IsBlockedByProvider { get; set; }

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public Guid ChildId { get; set; }

    public Guid ParentId { get; set; }

    public WorkshopCard Workshop { get; set; }

    public ChildDto Child { get; set; }

    public ParentDTO Parent { get; set; }
}

public static class ApplicationDtoExtensions
{
    public static ApplicationDto ToDto(this OutOfSchool.Services.Models.Application application)
        => new()
        {
            Id = application.Id,
            Status = application.Status,
            RejectionMessage = application.RejectionMessage,
            CreationTime = application.CreationTime,
            ApprovedTime = application.ApprovedTime,
            IsBlockedByProvider = application.IsBlockedByProvider,
            WorkshopId = application.WorkshopId,
            ChildId = application.ChildId,
            ParentId = application.ParentId,
            Workshop = application.Workshop.ToCard(),
            Child = application.Child.ToDto(),
            Parent = application.Parent.ToDto(),
        };

    public static List<ApplicationDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Application> list)
        => list.MapToList(ToDto);
}