using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.WebApi.Models;

public class ParentCard : WorkshopCard
{
    [Required]
    public Guid ChildId { get; set; }

    [Required]
    public Guid ApplicationId { get; set; }

    public ApplicationStatus Status { get; set; }
}