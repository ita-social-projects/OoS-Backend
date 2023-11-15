using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models.Workshops;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopProviderViewCardGenerator
{
    private static readonly Faker<WorkshopProviderViewCard> Faker = new Faker<WorkshopProviderViewCard>()
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Address, f => AddressDtoGenerator.Generate())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.TakenSeats, f => f.Random.UInt(0, 5))
        .RuleFor(x => x.Rating, f => f.Random.Float(0, 5))
        .RuleFor(x => x.NumberOfRatings, f => f.Random.Int(0))
        .RuleFor(x => x.AmountOfPendingApplications, f => f.Random.Int(0))
        .RuleFor(x => x.Status, f => f.PickRandom<WorkshopStatus>())
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopProviderViewCard();
            WorkshopBaseCardGenerator.Populate(dto);
            return dto;
        });
 
    public static WorkshopProviderViewCard Generate() => Faker.Generate();

    public static List<WorkshopProviderViewCard> Generate(int count) => Faker.Generate(count);
}