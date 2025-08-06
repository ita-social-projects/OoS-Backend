using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopMainRequiredPropertiesDtoGenerator
{
    public static readonly Faker<WorkshopMainRequiredPropertiesDto> Faker = new Faker<WorkshopMainRequiredPropertiesDto>()
        .RuleFor(w => w.Id, f => f.Random.Guid())
        .RuleFor(w => w.Title, f => f.Name.FullName())
        .RuleFor(w => w.ShortTitle, f => f.Name.LastName())
        .RuleFor(w => w.MinAge, f => f.Random.Int(5, 9))
        .RuleFor(w => w.MaxAge, f => f.Random.Int(10, 13))
        .RuleFor(w => w.AvailableSeats, f => f.Random.UInt(0, 15))
        .RuleFor(w => w.CompetitiveSelection, f => true)
        .RuleFor(w => w.CompetitiveSelectionDescription, f => f.Lorem.Paragraph())
        .RuleFor(w => w.ProviderId, f => f.Random.Guid())
        .RuleFor(w => w.DateTimeRanges, f => DateTimeRangeDtoGenerator.Generate(4))
        .RuleFor(w => w.FormOfLearning, f => f.PickRandom<FormOfLearning>())
        .RuleFor(w => w.LanguageOfEducationId, f => f.Random.Long(1, uint.MaxValue))       
        .RuleFor(w => w.StudyPeriodDates, f => StudyPeriodDatesDtoGenerator.Generate())
        .RuleFor(w => w.Base64CoverImage, f => f.Random.Word());

    public static WorkshopMainRequiredPropertiesDto Generate() => Faker.Generate();

    public static List<WorkshopMainRequiredPropertiesDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopMainRequiredPropertiesDto dto) => Faker.Populate(dto);
}