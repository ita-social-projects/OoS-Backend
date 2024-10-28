using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderBaseDto : IHasCoverImage, IHasImages
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full Title is required")]
    [DataType(DataType.Text)]
    [MinLength(Constants.MinProviderFullTitleLength)]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string FullTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Short Title is required")]
    [DataType(DataType.Text)]
    [MinLength(Constants.MinProviderShortTitleLength)]
    [MaxLength(Constants.MaxProviderShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string FullTitleEn { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxProviderShortTitleLength)]
    public string ShortTitleEn { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Website { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
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
    [CustomAge(MinAge = Constants.AdultAge, ErrorMessage = Constants.DayOfBirthErrorMessage)]
    public DateTime? DirectorDateOfBirth { get; set; } = default;

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [Required(ErrorMessage = "The phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(Constants.MaxProviderFounderLength)]
    public string Founder { get; set; } = string.Empty;

    public long TypeId { get; set; }

    public ProviderTypeDto Type { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;

    [MaxLength(500)]
    public string StatusReason { get; set; }

    [MaxLength(30)]
    public string License { get; set; }

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus LicenseStatus { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }

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
