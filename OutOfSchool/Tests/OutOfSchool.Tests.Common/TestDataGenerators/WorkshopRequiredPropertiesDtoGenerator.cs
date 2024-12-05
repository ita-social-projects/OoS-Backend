using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopRequiredPropertiesDtoGenerator
{
    private static readonly Faker<WorkshopRequiredPropertiesDto> Faker = new Faker<WorkshopRequiredPropertiesDto>()
        .RuleFor(w => w.ShortStay, f => f.Random.Bool())
        .RuleFor(w => w.IsSelfFinanced, f => f.Random.Bool())
        .RuleFor(w => w.IsSpecial, f => f.Random.Bool())
        .RuleFor(w => w.SpecialNeedsId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.IsInclusive, f => f.Random.Bool())
        .RuleFor(w => w.EducationalShiftId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.LanguageOfEducationId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.TypeOfAgeCompositionId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.EducationalDisciplines, f => f.Random.Guid())
        .RuleFor(w => w.CategoryId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.GropeTypeId, f => f.Random.UInt(0, uint.MaxValue))
        .RuleFor(w => w.MemberOfWorkshopId, f => null)
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