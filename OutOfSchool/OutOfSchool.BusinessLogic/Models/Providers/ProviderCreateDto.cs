using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderCreateDto : ProviderBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }
}

public static class ProviderCreateDtoExtensions
{
    public static Provider ToModel(this ProviderCreateDto dto)
        => new()
        {
            Id = dto.Id,
            FullTitle = dto.FullTitle,
            ShortTitle = dto.ShortTitle,
            FullTitleEn = dto.FullTitleEn,
            ShortTitleEn = dto.ShortTitleEn,
            GeneralWorkSchedule = dto.GeneralWorkSchedule,
            TypeId = dto.TypeId,
            Type = dto.Type?.ToModel(),
            License = dto.License,
            InstitutionStatusId = dto.InstitutionStatusId,
            InstitutionId = dto.InstitutionId,
            InstitutionType = dto.InstitutionType,
            ProviderSectionItems = dto.ProviderSectionItems?.ToModel(),
            UsesOutsourcingServices = dto.UsesOutsourcingServices,
            InstitutionCode = dto.InstitutionCode,
            IsStructuralUnit = dto.IsStructuralUnit,
            IsLocatedInMountainousArea = dto.IsLocatedInMountainousArea,
            Ownership = dto.Ownership,
            ParentProviderId = dto.ParentProviderId            
        };
}
