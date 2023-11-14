using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models.Workshops;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopCardGenerator
{
    private static readonly Faker<WorkshopCard> Faker = new Faker<WorkshopCard>()
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.InstitutionId, f => f.Random.Guid())
        .RuleFor(x => x.Institution, f => f.Lorem.Word())
        .RuleFor(x => x.InstitutionHierarchyId, f => f.Random.Guid())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Address, f => AddressDtoGenerator.Generate())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.TakenSeats, f => f.Random.UInt(0, 5))
        .RuleFor(x => x.Rating, f => f.Random.Float(0, 5))
        .RuleFor(x => x.NumberOfRatings, f => f.Random.Int(0))
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopCard();
            WorkshopBaseCardGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopCard Generate() => Faker.Generate();

    public static List<WorkshopCard> Generate(int count) => Faker.Generate(count);
}