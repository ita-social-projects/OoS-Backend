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
        @"^\d{8}$",
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

public static class ProviderInfoDtoExtensions
{
    public static ProviderInfoBaseDto ToBaseInfoDto(this Provider model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
        };

    public static ProviderInfoDto ToInfoDto(this Provider model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Ownership = model.Ownership,
            FullTitle = model.FullTitle,
            ShortTitle = model.ShortTitle,
            FullTitleEn = model.FullTitleEn,
            ShortTitleEn = model.ShortTitleEn,
            Edrpou = model.Edrpou,
            GeneralWorkSchedule = model.GeneralWorkSchedule,
            Type = model.Type.Name,
            Status = model.Status,
            CoverImageId = model.CoverImageId,
            ImageIds = model.Images.Select(x => x.ExternalStorageId).ToArray(),
            IsBlocked = model.IsBlocked,
            Institution = model.Institution.Title,
            InstitutionType = model.InstitutionType,
            ProviderSectionItems = model.ProviderSectionItems.ToInfoDto(),
            Contacts = model.Contacts.ToInfoDto()
        };

    public static List<ProviderInfoBaseDto> ToBaseOrInfoDto(this IEnumerable<Provider> list)
        => list.MapToList(x => x.IsDeleted ? x.ToBaseInfoDto() : x.ToInfoDto());
}