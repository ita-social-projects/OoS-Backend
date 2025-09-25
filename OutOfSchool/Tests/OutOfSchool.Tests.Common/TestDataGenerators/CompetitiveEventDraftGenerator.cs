using Bogus;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class CompetitiveEventDraftGenerator
{
    private static readonly Faker<CompetitiveEventDraft> Faker = new Faker<CompetitiveEventDraft>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.CompetitiveEventId, f => f.Random.Guid())
        .RuleFor(x => x.DraftStatus, CompetitiveEventDraftStatus.Draft)
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long(1, 100000))
        .RuleFor(x => x.CoverageId, f => f.Random.Int(1, 5))
        .RuleFor(x => x.CompetitiveEventAccountingTypeId, f => f.Random.Int(1, 10))
        .RuleFor(x => x.CoverImageId, f => f.Lorem.Word())
        .RuleFor(x => x.CompetitiveEventDraftContent, CompetitiveEventDraftContentGenerator.Generate());
    public static CompetitiveEventDraft Generate() => Faker.Generate();

    public static List<CompetitiveEventDraft> Generate(int count) => Faker.Generate(count);
}