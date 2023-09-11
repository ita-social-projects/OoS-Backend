using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopV2DtoGenerator
{
    private static readonly Faker<WorkshopV2Dto> Faker = new Faker<WorkshopV2Dto>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid), f => f.Random.Guid())
        .RuleForType(typeof(long), f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string), f => f.Lorem.Word())
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.Phone, f => f.Phone.ToString())
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.Website, f => f.Internet.Url())
        .RuleFor(x => x.Facebook, f => f.Person.UserName)
        .RuleFor(x => x.Instagram, f => f.Person.UserName)
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.DateTimeRanges, f => f.Make(new Random().Next(1, 4), () =>
            new DateTimeRangeDto()
            {
                Id = f.Random.Long(),
                StartTime = f.Date.Timespan(),
                EndTime = f.Date.Timespan(),
                Workdays = new List<DaysBitMask>()
            }))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence())
        .RuleFor(x => x.WorkshopDescriptionItems, f => f.Make(new Random().Next(1, 6), () =>
            new WorkshopDescriptionItemDto()
            {
                Id = Guid.NewGuid(),
                SectionName = f.Lorem.Sentence(),
                Description = f.Lorem.Paragraph(),
            }))
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.DisabilityOptionsDesc, f => f.Lorem.Sentence())
        .RuleFor(x => x.InstitutionId, f => f.Random.Guid())
        .RuleFor(x => x.Institution, f => f.Lorem.Word())
        .RuleFor(x => x.InstitutionHierarchyId, f => f.Random.Guid())
        .RuleFor(x => x.InstitutionHierarchy, f => f.Lorem.Word())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Keywords, f => f.Make(new Random().Next(1, 10), () => f.Lorem.Word()))
        .RuleFor(x => x.AddressId, f => f.Random.Number(1, 1000))
        .RuleFor(x => x.Address, f =>
            new AddressDto()
            {
                BuildingNumber = f.Random.Byte().ToString(),
            })
        .RuleFor(x => x.Teachers, f => f.Make(new Random().Next(1, 3), () => new TeacherDTO()))
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.TakenSeats, f => f.Random.UInt(0, 5))
        .RuleFor(x => x.Rating, f => f.Random.Float(0, 5))
        .RuleFor(x => x.NumberOfRatings, f => f.Random.Int(0))
        .RuleFor(x => x.Status, f => f.PickRandom<WorkshopStatus>())
        .RuleFor(x => x.IsBlocked, f => f.Random.Bool())
        .RuleFor(x => x.ProviderOwnership, f => f.PickRandom<OwnershipType>())
        .RuleFor(x => x.ProviderStatus, f => f.PickRandom<ProviderStatus>())
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ImageIds, _ => new List<string>());

    public static WorkshopV2Dto Generate() => Faker.Generate();

    public static List<WorkshopV2Dto> Generate(int count) => Faker.Generate(count);
}