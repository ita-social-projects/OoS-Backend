using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderDto : ProviderBaseDto, IHasRating
{
    [Required]
    public string Edrpou { get; set; }

    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }

    public bool IsBlocked { get; set; }

    [MaxLength(500)]
    public string BlockReason { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    public string BlockPhoneNumber { get; set; } = string.Empty;
}

public static class ProviderDtoExtensions
{
    public static ProviderDto ToDto(this Provider model)
        => new()
        {
            Id = model.Id,
            FullTitle = model.FullTitle,
            ShortTitle = model.ShortTitle,
            FullTitleEn = model.FullTitleEn,
            ShortTitleEn = model.ShortTitleEn,
            GeneralWorkSchedule = model.GeneralWorkSchedule,
            TypeId = model.TypeId,
            Type = model.Type?.ToDto(),
            Status = model.Status,
            StatusReason = model.StatusReason,
            License = model.License,
            LicenseStatus = model.LicenseStatus,
            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(x => x.ExternalStorageId).ToArray() ?? [],
            InstitutionStatusId = model.InstitutionStatusId,
            InstitutionId = model.InstitutionId,
            Institution = model.Institution?.ToDto(),
            InstitutionType = model.InstitutionType,
            ProviderSectionItems = model.ProviderSectionItems?.ToNotDeletedDto() ?? [],
            UsesOutsourcingServices = model.UsesOutsourcingServices,
            InstitutionCode = model.InstitutionCode,
            IsStructuralUnit = model.IsStructuralUnit,
            IsLocatedInMountainousArea = model.IsLocatedInMountainousArea,
            Contacts = model.Contacts?.ToDto() ?? [],
            Edrpou = model.Edrpou,
            Ownership = model.Ownership,
            IsBlocked = model.IsBlocked,
            BlockReason = model.BlockReason,
            BlockPhoneNumber = model.BlockPhoneNumber,
            ParentProviderId = model.ParentProviderId,
            ParentProviderName = model.ParentProvider != null ? model.ParentProvider.FullTitle : null
        };

    public static List<ProviderDto> ToDto(this IEnumerable<Provider> list)
        => list.MapToList(ToDto);
}