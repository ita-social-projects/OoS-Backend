﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models.ProvidersInfo;

public class ProviderInfoDto : ProviderInfoBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }

    [Required(ErrorMessage = "Full Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(120)]
    [MinLength(1)]
    public string FullTitle { get; set; }

    [Required(ErrorMessage = "Short Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(60)]
    [MinLength(1)]
    public string ShortTitle { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Website { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Instagram { get; set; } = string.Empty;

    // TODO: validate regex with unit tests
    [Required(ErrorMessage = "EDRPOU/INP code is required")]
    [RegularExpression(
        @"^(\d{8}|\d{10})$",
        ErrorMessage = "EDRPOU/IPN code must contain 8 or 10 digits")]
    public string EdrpouIpn { get; set; }

    [MaxLength(50)]
    [Required(ErrorMessage = "The name of the director is required")]
    public string Director { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "The date of birth is required")]
    public DateTime? DirectorDateOfBirth { get; set; } = default;

    [DataType(DataType.PhoneNumber)]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    [Required(ErrorMessage = "The phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Founder { get; set; } = string.Empty;

    public ProviderTypeDto Type { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;

    [MaxLength(30)]
    public string License { get; set; }

    public bool IsBlocked { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    public AddressDto LegalAddress { get; set; }

    public AddressDto ActualAddress { get; set; }

    public InstitutionDto Institution { get; set; }

    [Required]
    [EnumDataType(typeof(InstitutionType), ErrorMessage = Constants.EnumErrorMessage)]
    public InstitutionType InstitutionType { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<ProviderSectionItemDto> ProviderSectionItems { get; set; }


}
