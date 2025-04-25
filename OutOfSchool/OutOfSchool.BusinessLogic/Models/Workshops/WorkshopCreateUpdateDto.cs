using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Models.Workshops;
public class WorkshopCreateUpdateDto : WorkshopBaseDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> TagIds { get; set; } = [];
}

public static class WorkshopCreateUpdateDtoExtensions
{
    public static Workshop SetToModel(this WorkshopCreateUpdateDto dto, Workshop model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.MinAge = dto.MinAge ?? default;
        model.MaxAge = dto.MaxAge ?? default;
        model.DateTimeRanges = dto.DateTimeRanges?.ToModel()
            .Concat(model.DateTimeRanges ?? [])
            .Distinct(new DateTimeRangeComparerWithoutFK())
            .ToList() ?? [];
        model.IsPaid = dto.IsPaid;
        model.Price = dto.Price ?? default;
        model.PayRate = dto.PayRate ?? default;
        model.FormOfLearning = dto.FormOfLearning;
        model.AvailableSeats = dto.AvailableSeats ?? default;
        model.CompetitiveSelection = dto.CompetitiveSelection;
        model.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
        model.WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToModel() ?? [];
        model.InstitutionHierarchyId = dto.InstitutionHierarchyId;
        model.DefaultTeacher = dto.DefaultTeacher?.ToModel(dto.DefaultTeacher.Id, dto.DefaultTeacher.WorkshopId);
        model.Keywords = string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []);
        model.ProviderId = dto.ProviderId;
        model.ActiveFrom = dto.ActiveFrom;
        model.ActiveTo = dto.ActiveTo;
        model.IsSelfFinanced = false;
        model.SpecialNeedsType = SpecialNeedsType.None;
        model.IsInclusive = false;
        model.EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription;
        model.AreThereBenefits = dto.AreThereBenefits;
        model.PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation;
        model.EducationalShift = EducationalShift.First;
        model.LanguageOfEducationId = dto.LanguageOfEducationId;
        model.AgeComposition = AgeComposition.SameAge;
        model.Coverage = dto.Coverage;
        model.WorkshopType = dto.WorkshopType;
        model.DefaultTeacherId = dto.DefaultTeacherId;
        model.ParentWorkshopId = dto.ParentWorkshopId;

        return model;
    }

    public static Workshop ToModel(this WorkshopCreateUpdateDto dto) 
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            DateTimeRanges = dto.DateTimeRanges?.ToModel() ?? [],
            IsPaid = dto.IsPaid,
            Price = dto.Price ?? default,
            PayRate = dto.PayRate ?? default,
            FormOfLearning = dto.FormOfLearning,
            AvailableSeats = dto.AvailableSeats ?? default,
            CompetitiveSelection = dto.CompetitiveSelection,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToModel() ?? [],
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            DefaultTeacher = dto.DefaultTeacher?.ToModel(dto.DefaultTeacher.Id, dto.DefaultTeacher.WorkshopId),
            Keywords = string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []),
            ProviderId = dto.ProviderId,
            ActiveFrom = dto.ActiveFrom,
            ActiveTo = dto.ActiveTo,
            IsSelfFinanced = false,
            SpecialNeedsType = SpecialNeedsType.None,
            IsInclusive = false,
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            EducationalShift = EducationalShift.First,
            LanguageOfEducationId = dto.LanguageOfEducationId,
            AgeComposition = AgeComposition.SameAge,
            Coverage = dto.Coverage,
            WorkshopType = dto.WorkshopType,
            DefaultTeacherId = dto.DefaultTeacherId,
            ParentWorkshopId = dto.ParentWorkshopId,
        };
}