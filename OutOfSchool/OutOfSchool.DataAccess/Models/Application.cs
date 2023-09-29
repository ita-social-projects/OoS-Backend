﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Application : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public ApplicationStatus Status { get; set; }

    [MaxLength(500)]
    public string RejectionMessage { get; set; }

    public DateTimeOffset CreationTime { get; set; }

    public DateTimeOffset? ApprovedTime { get; set; }

    public bool IsBlocked { get; set; }

    public Guid WorkshopId { get; set; }

    public Guid ChildId { get; set; }

    public Guid ParentId { get; set; }

    public virtual Workshop Workshop { get; set; }

    public virtual Child Child { get; set; }

    public virtual Parent Parent { get; set; }

    [NotMapped]
    public static readonly ApplicationStatus[] ValidApplicationStatuses = { ApplicationStatus.Approved, ApplicationStatus.StudyingForYears, ApplicationStatus.Completed};
}