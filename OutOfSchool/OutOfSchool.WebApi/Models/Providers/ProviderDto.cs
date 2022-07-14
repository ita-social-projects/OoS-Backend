using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(60)]
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
    public string Director { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DirectorDateOfBirth { get; set; } = default;

    [DataType(DataType.PhoneNumber)]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Founder { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderType), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderType Type { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; }

    [MaxLength(30)]
    public string License { get; set; }

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus LicenseStatus { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IFormFile CoverImage { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<IFormFile> ImageFiles { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    // TODO: Does not used by front-end, can be removed.
    //       Unit test should be updated
    [Required]
    public string UserId { get; set; }

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public AddressDto LegalAddress { get; set; }

    public AddressDto ActualAddress { get; set; }

    public long? InstitutionStatusId { get; set; } = default;

    public Guid? InstitutionId { get; set; }

    public InstitutionDto Institution { get; set; }

    [Required]
    [EnumDataType(typeof(InstitutionType), ErrorMessage = Constants.EnumErrorMessage)]
    public InstitutionType InstitutionType { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<ProviderSectionItemDto> ProviderSectionItems { get; set; }
}