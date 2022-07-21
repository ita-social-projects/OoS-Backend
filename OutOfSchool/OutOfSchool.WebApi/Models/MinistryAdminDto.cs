﻿using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models;

public class MinistryAdminDto
{
    public string Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }
}