using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using System.Collections.Generic;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopRequiredPropertiesDtoGenerator
{
    private static readonly Faker<WorkshopRequiredPropertiesDto> Faker = new Faker<WorkshopRequiredPropertiesDto>()
        .RuleFor(w => w.ShortStay, f => f.Random.Bool())
        .RuleFor(w => w.IsSelfFinanced, f => f.Random.Bool())
        .RuleFor(w => w.IsSpecial, f => f.Random.Bool())
        .RuleFor(w => w.SpecialNeedsType, f => f.Random.Enum<SpecialNeedsType>())
        .RuleFor(w => w.IsInclusive, f => f.Random.Bool())
        .RuleFor(w => w.EducationalShift, f => f.Random.Enum<EducationalShift>())
        .RuleFor(w => w.LanguageOfEducationId, f => f.Random.UInt(1, uint.MaxValue))
        .RuleFor(w => w.AgeComposition, f => f.Random.Enum<AgeComposition>())
        .RuleFor(w => w.WorkshopType, f => f.Random.Enum<WorkshopType>())
        .RuleFor(w => w.ParentWorkshopId, f => null)
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopRequiredPropertiesDto();
            WorkshopMainRequiredPropertiesDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopRequiredPropertiesDto Generate() => Faker.Generate();

    public static List<WorkshopRequiredPropertiesDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopRequiredPropertiesDto dto) => Faker.Populate(dto);
}