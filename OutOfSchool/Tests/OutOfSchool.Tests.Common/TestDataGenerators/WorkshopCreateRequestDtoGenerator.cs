using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopCreateRequestDtoGenerator
{
    private static readonly Faker<WorkshopCreateRequestDto> Faker = new Faker<WorkshopCreateRequestDto>()
        .RuleFor(x => x.TagIds, f => f.Make(5, () => f.Random.Long(1, 100)))
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopCreateRequestDto();
            WorkshopRequiredPropertiesDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopCreateRequestDto Generate() => Faker.Generate();

    public static List<WorkshopCreateRequestDto> Generate(int count) => Faker.Generate(count); 
    
    public static WorkshopCreateRequestDto FromModel(this Workshop model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            DateTimeRanges = model.DateTimeRanges?.ToDto(),
            FormOfLearning = model.FormOfLearning,
            CompetitiveSelection = model.CompetitiveSelection,
            CompetitiveSelectionDescription = model.CompetitiveSelectionDescription,
            ProviderId = model.ProviderId,

            WorkshopType = model.WorkshopType,
            ParentWorkshopId = model.ParentWorkshopId,
            IsPaid = model.IsPaid,
            Price = model.Price,
            PayRate = model.PayRate,
            AreThereBenefits = model.AreThereBenefits,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = model.WorkshopDescriptionItems?.ToDto(),
            InstitutionHierarchyId = model.InstitutionHierarchyId,
            Keywords = model.Keywords?.Split(OutOfSchool.Common.Constants.MappingSeparator, StringSplitOptions.RemoveEmptyEntries).ToList(),
            EnrollmentProcedureDescription = model.EnrollmentProcedureDescription,
            Coverage = model.Coverage,

            DefaultTeacher = model.DefaultTeacher?.ToDto(),
            DefaultTeacherId = model.DefaultTeacherId,
            AvailableSeats = model.AvailableSeats == 0 ? uint.MaxValue : model.AvailableSeats,
        };

    public static void Populate(WorkshopCreateRequestDto dto) => Faker.Populate(dto);
}
