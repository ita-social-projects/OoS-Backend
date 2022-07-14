using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopStatusDto
{
    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; }
}