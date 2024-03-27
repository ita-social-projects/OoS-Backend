using Bogus;
using OutOfSchool.Services.Models;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class WorkshopDescriptionItemGenerator
{
    private static readonly Faker<WorkshopDescriptionItem> Faker = new Faker<WorkshopDescriptionItem>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.SectionName, f => f.Lorem.Sentence())
        .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
        .RuleFor(x => x.WorkshopId, f => f.Random.Guid());

    public static WorkshopDescriptionItem Generate() => Faker.Generate();

    public static List<WorkshopDescriptionItem> Generate(int count) => Faker.Generate(count);
}