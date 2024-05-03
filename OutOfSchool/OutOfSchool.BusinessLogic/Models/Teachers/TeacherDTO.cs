﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class TeacherDTO
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = "First name cannot contains digits")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = "Last name cannot contains digits")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = "Middle name cannot contains digits")]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; } = default;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public string CoverImageId { get; set; }

    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    public Guid WorkshopId { get; set; }
}