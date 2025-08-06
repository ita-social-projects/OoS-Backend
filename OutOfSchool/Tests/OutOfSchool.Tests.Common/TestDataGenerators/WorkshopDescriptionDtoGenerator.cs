using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopDescriptionDtoGenerator
{
    private static readonly Faker<WorkshopDescriptionDto> Faker = new Faker<WorkshopDescriptionDto>()

        .RuleFor(w => w.WorkshopDescriptionItems, f => WorkshopDescriptionItemDtoGenerator.Generate(4))
        .RuleFor(w => w.DirectionIds, f => f.Make(3, () => f.Random.Long(1, 50)))
        .RuleFor(w => w.Keywords, f => f.Make(3, () => f.Random.Word()))
        .RuleFor(w => w.EnrollmentProcedureDescription, f => f.Lorem.Paragraph())
        .RuleFor(w => w.Coverage, f => f.Random.Enum<Coverage>())
        .RuleFor(w => w.TagIds, f => f.Make(3, () => f.Random.Long(1, 50)))
        .RuleFor(w => w.CompetitiveSelection, f => true)
        .RuleFor(w => w.CompetitiveSelectionDescription, f => f.Lorem.Paragraph())
        .RuleFor(w => w.Base64ImageFiles, f => f.Make(5, () => Convert.ToBase64String(f.Random.Bytes(100))))
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopDescriptionDto();
            WorkshopMainRequiredPropertiesDtoGenerator.Populate(dto);
            WorkshopRequiredPropertiesDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopDescriptionDto Generate() => Faker.Generate();

    public static List<WorkshopDescriptionDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopDescriptionDto dto) => Faker.Populate(dto);
}
