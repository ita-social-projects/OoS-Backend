using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class WorkshopProviderViewCardGenerator
{
    private static readonly Faker<WorkshopProviderViewCard> Faker = new Faker<WorkshopProviderViewCard>()
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
        .RuleFor(x => x.WorkshopId, _ => Guid.NewGuid())
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(0, 100))
        .RuleFor(x => x.TakenSeats, f => f.Random.UInt(0, 100))
        .RuleFor(x => x.AmountOfPendingApplications, f => f.Random.Int(0, 100))
        .RuleFor(x => x.UnreadMessages, f => f.Random.Int(0, 100));

    public static WorkshopProviderViewCard Generate() => Faker.Generate();

    public static List<WorkshopProviderViewCard> Generate(int count) => new List<WorkshopProviderViewCard>(count);
}
