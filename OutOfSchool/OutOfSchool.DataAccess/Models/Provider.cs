using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Models;

public class Provider : BusinessEntity, IImageDependentEntity<Provider>, IHasEntityImages<Provider>, IHasContacts
{
    [Required]
    [DataType(DataType.Text)]
    [MinLength(Constants.MinProviderFullTitleLength)]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string FullTitle { get; set; } = string.Empty;

    [Required]
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

    [Required]
    [RegularExpression(
        @"^\d{8}$",
        ErrorMessage = "EDRPOU code must contain 8 digits")]
    [MaxLength(8)]
    public string Edrpou { get; set; }

    [Required]
    public OwnershipType Ownership { get; set; }

    [Required]
    public long TypeId { get; set; }

    [Required]
    public ProviderStatus Status { get; set; }

    [MaxLength(500)]
    public string StatusReason { get; set; }

    [MaxLength(30)]
    public string License { get; set; }

    public ProviderLicenseStatus LicenseStatus { get; set; }

    [MaxLength(256)]
    public string LicenseLimits { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LicenseIssuanceDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LicenseExpirationDate { get; set; }

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [Required(ErrorMessage = "PhoneNumber is required")]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string BlockPhoneNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BlockReason { get; set; }

    [Required]
    public string UserId { get; set; }

    public long? InstitutionStatusId { get; set; }

    public Guid? InstitutionId { get; set; }

    [Required]
    public InstitutionType InstitutionType { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [NotMapped]
    public static readonly ProviderStatus[] ValidProviderStatuses = { ProviderStatus.Approved, ProviderStatus.Recheck };
    
    public bool UsesOutsourcingServices { get; set; } = false;
    
    [DataType(DataType.Text)]
    [MaxLength(500)]
    public string GeneralWorkSchedule { get; set; }

    #region Field that should gradualy become required after release

    [MaxLength(256)]
    public string InstitutionCode { get; set; } = string.Empty;

    public bool IsStructuralUnit { get; set; } = false;

    public bool IsLocatedInMountainousArea { get; set; } = false;

    /*
    TODO: Should become required when external registry is active.
     currently is a placeholder property.
    */
    public Guid? ExternalId { get; set; } = null;

    #endregion
    
    #region Navigation properties
    
    public virtual User User { get; set; }
    
    public virtual ProviderType Type { get; set; }
    
    public virtual List<Workshop> Workshops { get; set; }

    public virtual List<WorkshopDraft> WorkshopDrafts { get; set; }
    
    public virtual InstitutionStatus InstitutionStatus { get; set; }
    
    public virtual ICollection<Position> Positions { get; set; }
    
    public virtual List<Image<Provider>> Images { get; set; }

    public virtual ICollection<ProviderSectionItem> ProviderSectionItems { get; set; }

    public virtual Institution Institution { get; set; }

    public virtual ICollection<Employee> Employees { get; set; }
    
    #endregion

    #region Owned entities

    public List<Contacts> Contacts { get; set; } = [];

    #endregion
}