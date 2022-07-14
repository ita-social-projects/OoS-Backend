﻿using System;
using System.ComponentModel.DataAnnotations;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class ShortApplicationDto
{
    public Guid Id { get; set; }

    [Required]
    [EnumDataType(typeof(ApplicationStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [MaxLength(500)]
    public string RejectionMessage { get; set; }
}
