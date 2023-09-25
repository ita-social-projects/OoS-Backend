using Bogus;
using OutOfSchool.WebApi.Models.Workshops;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopDescriptionItemDtoGenerator
{
    private static readonly Faker<WorkshopDescriptionItemDto> Faker = new Faker<WorkshopDescriptionItemDto>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.SectionName, f => f.Lorem.Sentence())
        .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
        .RuleFor(x => x.WorkshopId, f => f.Random.Guid());

    public static WorkshopDescriptionItemDto Generate() => Faker.Generate();

    public static List<WorkshopDescriptionItemDto> Generate(int count) => Faker.Generate(count);
}