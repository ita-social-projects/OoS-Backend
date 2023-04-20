using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class Provider : IKeyedEntity<Guid>, IImageDependentEntity<Provider>
{
    public Guid Id { get; set; }

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

    [Required(ErrorMessage = "EDRPOU/IPN code is required")]
    [RegularExpression(
        @"^(\d{8}|\d{10})$",
        ErrorMessage = "EDRPOU/IPN code must contain 8 or 10 digits")]
    [MaxLength(12)]
    public string EdrpouIpn { get; set; }

    [MaxLength(50)]
    [Required(ErrorMessage = "Director is required")]
    public string Director { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Column(TypeName = "date")]
    [Required(ErrorMessage = "DirectorDateOfBirth is required")]
    public DateTime? DirectorDateOfBirth { get; set; } = default;

    [DataType(DataType.PhoneNumber)]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    [Required(ErrorMessage = "PhoneNumber is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Founder { get; set; } = string.Empty;

    [Required]
    public OwnershipType Ownership { get; set; }

    public virtual ProviderType Type { get; set; }

    [Required]
    public long TypeId { get; set; }

    [Required]
    public ProviderStatus Status { get; set; }

    [MaxLength(500)]
    public string StatusReason { get; set; }

    [MaxLength(30)]
    public string License { get; set; }

    public ProviderLicenseStatus LicenseStatus { get; set; }

    public bool IsBlocked { get; set; } = false;

    [MaxLength(500)]
    public string BlockReason { get; set; }

    [Required]
    public string UserId { get; set; }

    public virtual List<Workshop> Workshops { get; set; }

    public virtual User User { get; set; }

    public long? ActualAddressId { get; set; }

    public virtual Address ActualAddress { get; set; }

    [Required]
    public long LegalAddressId { get; set; }

    [Required]
    public virtual Address LegalAddress { get; set; }

    public long? InstitutionStatusId { get; set; }

    public virtual InstitutionStatus InstitutionStatus { get; set; }

    public Guid? InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    public virtual ICollection<ProviderAdmin> ProviderAdmins { get; set; }

    [Required]
    public InstitutionType InstitutionType { get; set; }

    public string CoverImageId { get; set; }

    public virtual List<Image<Provider>> Images { get; set; }

    public virtual ICollection<ProviderSectionItem> ProviderSectionItems { get; set; }

    [NotMapped]
    public static readonly ProviderStatus[] ValidProviderStatuses = { ProviderStatus.Approved, ProviderStatus.Recheck };
}