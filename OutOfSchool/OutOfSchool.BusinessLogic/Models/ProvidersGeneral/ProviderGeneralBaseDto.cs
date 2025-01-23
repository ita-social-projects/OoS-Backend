using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.ProvidersGeneral;
public class ProviderGeneralBaseDto : IHasCoverImage, IHasImages
{
    [Required(ErrorMessage = "Institution code is required")]
    [DataType(DataType.Text)]    
    public string InstitutionCode { get; set; }

    [Required(ErrorMessage = "Contact is required")]
    public Guid ContactId { get; set; }

    [Required(ErrorMessage = "Classifier type is required")]
    public long ClassifierTypeId { get; set; }

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
    public string BusinessName { get; set; }

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

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus LicenseStatus { get; set; }

    public string AdditionalDescription { get; set; }
    public string GeneralWorkSchedule { get; set; }
    public bool UsesOutsourcingServices { get; set; }

    public List<string> EducationDirections { get; set; } = new List<string>();
    public List<string> Specializations { get; set; } = new List<string>();

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType OwnershipType { get; set; }

    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning OrganizationType { get; set; }

    [EnumDataType(typeof(InstitutionType), ErrorMessage = Constants.EnumErrorMessage)]
    public InstitutionType InstitutionType { get; set; }

    public List<string> EquipmentMachines { get; set; } = new List<string>();
    
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public virtual IList<string> ImageIds { get; set; } = new List<string>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<ProviderSectionItemDto> ProviderSectionItems { get; set; }
}
