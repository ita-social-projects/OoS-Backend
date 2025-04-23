using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopCreateRequestDto : WorkshopContactsDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public TeacherDTO DefaultTeacher { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<TeacherDTO> Teachers { get; set; }

    public Guid? DefaultTeacherId { get; set; }
}

public static class WorkshopCreateRequestDtoExtensions
{
    public static Workshop ToModel(this WorkshopCreateRequestDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            DateTimeRanges = dto.DateTimeRanges.ToModel(),
            FormOfLearning = dto.FormOfLearning,
            CompetitiveSelection = dto.CompetitiveSelection,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            ProviderId = dto.ProviderId,

            WorkshopType = dto.WorkshopType,
            ParentWorkshopId = dto.ParentWorkshopId,
            IsPaid = dto.IsPaid,
            Price = dto.IsPaid ? dto.Price ?? 0 : 0,
            PayRate = dto.PayRate ?? default,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
           
            WorkshopDescriptionItems = dto.WorkshopDescriptionItems.ToModel(),
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            Keywords = string.Join(Constants.MappingSeparator, dto.Keywords.Distinct()),
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            Coverage = dto.Coverage,

            DefaultTeacher = dto.DefaultTeacher.ToModel(),
            DefaultTeacherId = dto.DefaultTeacherId,
        };
}