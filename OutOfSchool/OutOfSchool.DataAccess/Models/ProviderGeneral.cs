using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models;

public class ProviderGeneral : BusinessEntity, 
    IImageDependentEntity<ProviderGeneral>, 
    IHasEntityImages<ProviderGeneral>
{
    [Required(ErrorMessage = "Institution code is required")]
    [DataType(DataType.Text)]
    public string InstitutionCode { get; set; }

    [Required(ErrorMessage = "Contact is required")]
    public Guid ContactId { get; set; }    
    public virtual Contact Contact { get; set; }

    [Required(ErrorMessage = "Classifier type is required")]
    public long ClassifierTypeId {  get; set; }
    public virtual ProviderType ProviderType { get; set; }

    [Required(ErrorMessage = "Structural unit value is required")]
    public bool IsStructuralUnit { get; set; } = false;

    [Required(ErrorMessage = "Located in mountainousArea value is required")]
    public bool IsLocatedInMountainousArea { get; set; } = false;

    [Required(ErrorMessage = "External id is required")]
    public Guid ExternalId { get; set; }


    [Required(ErrorMessage = "Business Name is required")]
    [DataType(DataType.Text)]
    [MinLength(Constants.MinProviderFullTitleLength)]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string BusinessName { get; set; } = string.Empty;

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

    [DataType(DataType.PhoneNumber)]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [Required(ErrorMessage = "PhoneNumber is required")]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string BlockPhoneNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    [DataType(DataType.Text)]
    public string BlockReason { get; set; } = string.Empty;

    [NotMapped]
    public static readonly ProviderStatus[] ValidProviderStatuses = { ProviderStatus.Approved, ProviderStatus.Recheck };

    public virtual ICollection<Employee> Employees { get; set; }

    public virtual ICollection<Position> Positions { get; set; }
        

    [DataType(DataType.Text)]
    public string ProviderIdAdministrative { get; set; }
    
    [DataType(DataType.Text)]
    public string ProviderIdDepartmental { get; set; }


    [DataType(DataType.Text)]
    public string LicenseSeriesNumber { get; set; }
    
    [DataType(DataType.Text)]
    public string LicenseLimits { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LicenseIssuanceDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LicenseExpirationDate { get; set; }

    [MaxLength(30)]
    public ProviderLicenseStatus LicenseStatus { get; set; } = ProviderLicenseStatus.NotProvided;


    [DataType(DataType.Text)]
    public string AdditionalDescription { get; set; }

    [DataType(DataType.Text)]
    public string GeneralWorkSchedule { get; set; }

    public bool UsesOutsourcingServices { get; set; } = false;
    
    public List<string> EducationDirections { get; set; } = new List<string>();

    public List<string> Specializations { get; set; } = new List<string>();

    public OwnershipType OwnershipType { get; set; }
    public FormOfLearning OrganizationType { get; set; }
    public InstitutionType InstitutionType { get; set; } // Functioning form     
    public ProviderStatus Status { get; set; }

    public List<string> EquipmentMachines { get; set; } = new List<string>();

    public string CoverImageId { get; set; } = string.Empty;
    public List<Image<ProviderGeneral>> Images { get; set; } = new List<Image<ProviderGeneral>>();
    
    public ICollection<ProviderSectionItemOwned> SectionItems { get; set; } = new List<ProviderSectionItemOwned>();
}