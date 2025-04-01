using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Exported.Contacts;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Exported.Providers;

public class ProviderInfoDto : ProviderInfoBaseDto, IExternalRatingInfo
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
    
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string FullTitleEn { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxProviderShortTitleLength)]
    public string ShortTitleEn { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^(\d{8}\d{10})$",
        ErrorMessage = "EDRPOU code must contain 8 digits")]
    public string Edrpou { get; set; }
    
    [DataType(DataType.Text)]
    [MaxLength(500)]
    public string GeneralWorkSchedule { get; set; }

    public string Type { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;
    
    public IList<string> ImageIds { get; set; }

    public bool IsBlocked { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    public string Institution { get; set; }

    [Required]
    [EnumDataType(typeof(InstitutionType), ErrorMessage = Constants.EnumErrorMessage)]
    public InstitutionType InstitutionType { get; set; }

    public IEnumerable<ProviderSectionItemInfoDto> ProviderSectionItems { get; set; }
    
    public List<ContactsInfoDto> Contacts { get; set; }
}