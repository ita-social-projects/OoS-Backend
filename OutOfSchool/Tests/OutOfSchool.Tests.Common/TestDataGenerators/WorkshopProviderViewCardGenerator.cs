using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;
using System;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopProviderViewCardGenerator
{
    private static readonly Faker<WorkshopProviderViewCard> Faker = new Faker<WorkshopProviderViewCard>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid), f => f.Random.Guid())
        .RuleForType(typeof(long), f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string), f => f.Lorem.Word())
        .RuleFor(x => x.WorkshopId, f => f.Random.Guid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Address, f =>
            new AddressDto()
            {
                BuildingNumber = f.Random.Byte().ToString(),
            })
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.TakenSeats, f => f.Random.UInt(0, 5))
        .RuleFor(x => x.Rating, f => f.Random.Float(0, 5))
        .RuleFor(x => x.NumberOfRatings, f => f.Random.Int(0))
        .RuleFor(x => x.ProviderOwnership, f => f.PickRandom<OwnershipType>())
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.AmountOfPendingApplications, f => f.Random.Int(0))
        .RuleFor(x => x.Status, f => f.PickRandom<WorkshopStatus>());
 
    public static WorkshopProviderViewCard Generate() => Faker.Generate();

    public static List<WorkshopProviderViewCard> Generate(int count) => Faker.Generate(count);
}