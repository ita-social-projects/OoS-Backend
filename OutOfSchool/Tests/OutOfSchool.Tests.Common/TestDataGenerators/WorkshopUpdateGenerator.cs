using System;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops;
using System.Collections.Generic;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopUpdateDtoGenerator
{
    private static readonly Faker<WorkshopUpdateDto> Faker = new Faker<WorkshopUpdateDto>()
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
        .RuleFor(x => x.DateTimeRanges, f => DateTimeRangeDtoGenerator.Generate(4))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence())
        .RuleFor(x => x.WorkshopDescriptionItems, f => WorkshopDescriptionItemDtoGenerator.Generate(6))
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.DisabilityOptionsDesc, f => f.Lorem.Sentence())
        .RuleFor(x => x.InstitutionId, f => f.Random.Guid())
        .RuleFor(x => x.InstitutionHierarchyId, f => f.Random.Guid())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Keywords, f => f.Make(new Random().Next(1, 10), () => f.Lorem.Word()))
        .RuleFor(x => x.AddressId, f => f.Random.Number(1, 1000))
        .RuleFor(x => x.Address, f => AddressDtoGenerator.Generate())
        .RuleFor(x => x.Teachers, f => f.Make(new Random().Next(1, 3), () => new TeacherUpdateDto()))
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.TagIds, f => f.Make(5, () => f.Random.Long(1, 100)));
    
    public static WorkshopUpdateDto Generate() => Faker.Generate();

    public static List<WorkshopUpdateDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopUpdateDto dto) => Faker.Populate(dto);
}