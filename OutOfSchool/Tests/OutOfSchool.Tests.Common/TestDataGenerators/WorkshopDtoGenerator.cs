using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopDtoGenerator
{
    private static readonly Faker<WorkshopV2DTO> Faker = new Faker<WorkshopV2DTO>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid),f => f.Random.Guid())
        .RuleForType(typeof(long),f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string),f => f.Lorem.Word())
        .RuleFor(x => x.Website, f => f.Internet.Url())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.Phone, f => f.Phone.ToString())
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.WorkshopDescriptionItems, f => f.Make(new Random().Next(1, 6), () =>
            new WorkshopDescriptionItemDTO()
            {
                Id = Guid.NewGuid(),
                SectionName = f.Lorem.Sentence(),
                Description = f.Lorem.Paragraph(),
            }))
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.Keywords, f => f.Make(new Random().Next(1, 10), () => f.Lorem.Word()))
        .RuleFor(x => x.Address, f =>
            new AddressDto()
            {
                BuildingNumber = f.Random.Byte().ToString(),
            })
        .RuleFor(x => x.DateTimeRanges, f => f.Make(new Random().Next(1, 4), () =>
            new DateTimeRangeDto()
            {
                Id = f.Random.Long(),
                StartTime = f.Date.Timespan(),
                EndTime = f.Date.Timespan(),
                Workdays = new List<DaysBitMask>()
            }))
        .RuleFor(x => x.Teachers, f =>
            f.Make(new Random().Next(1, 3), () => new TeacherDTO()));
        
    public static WorkshopV2DTO Generate() => Faker.Generate();
}