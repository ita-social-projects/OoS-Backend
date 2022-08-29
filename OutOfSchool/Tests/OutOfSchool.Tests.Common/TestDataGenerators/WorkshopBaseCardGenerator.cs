using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopBaseCardGenerator
{
    private static readonly Faker<WorkshopBaseCard> Faker = new Faker<WorkshopBaseCard>()
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 18))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(1, 18))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ProviderOwnership, f => f.PickRandom<OwnershipType>())
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.ProviderId, _ => Guid.NewGuid())
        .RuleFor(x => x.WorkshopId, _=> Guid.NewGuid());

    public static WorkshopBaseCard Generate() => Faker.Generate();

    public static List<WorkshopBaseCard> Generate(int count) => Faker.Generate(count);
}