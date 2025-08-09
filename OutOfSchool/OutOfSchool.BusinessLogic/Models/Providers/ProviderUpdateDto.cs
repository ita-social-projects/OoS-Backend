namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderUpdateDto : ProviderBaseDto
{
}

public static class ProviderUpdateDtoExtensions
{
    public static Provider SetToModel(this ProviderUpdateDto dto, Provider model)
    {
        model.Id = dto.Id;
        model.FullTitle = dto.FullTitle;
        model.ShortTitle = dto.ShortTitle;
        model.FullTitleEn = dto.FullTitleEn;
        model.ShortTitleEn = dto.ShortTitleEn;
        model.GeneralWorkSchedule = dto.GeneralWorkSchedule;
        model.TypeId = dto.TypeId;
        model.Type = dto.Type?.ToModel();
        model.License = dto.License;
        model.InstitutionStatusId = dto.InstitutionStatusId;
        model.InstitutionId = dto.InstitutionId;
        model.InstitutionType = dto.InstitutionType;
        model.UsesOutsourcingServices = dto.UsesOutsourcingServices;
        model.InstitutionCode = dto.InstitutionCode;
        model.IsStructuralUnit = dto.IsStructuralUnit;
        model.IsLocatedInMountainousArea = dto.IsLocatedInMountainousArea;

        return model;
    }
}
