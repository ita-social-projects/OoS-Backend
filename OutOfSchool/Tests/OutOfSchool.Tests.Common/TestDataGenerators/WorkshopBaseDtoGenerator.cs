using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopBaseDtoGenerator
{
    public static readonly Faker<WorkshopBaseDto> Faker = new Faker<WorkshopBaseDto>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid), f => f.Random.Guid())
        .RuleForType(typeof(long), f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string), f => f.Lorem.Word())
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.DateTimeRanges, f => DateTimeRangeDtoGenerator.Generate(4))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.CompetitiveSelection, f => f.Random.Bool())
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence())
        .RuleFor(x => x.WorkshopDescriptionItems, f => WorkshopDescriptionItemDtoGenerator.Generate(6))
        .RuleFor(x => x.InstitutionId, f => f.Random.Guid())
        .RuleFor(x => x.Institution, f => f.Lorem.Word())
        .RuleFor(x => x.InstitutionHierarchyId, f => f.Random.Guid())
        .RuleFor(x => x.InstitutionHierarchy, f => f.Lorem.Word())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.Keywords, f => f.Make(new Random().Next(1, 10), () => f.Lorem.Word()))
        .RuleFor(x => x.Teachers, f => f.Make(new Random().Next(1, 3), () => new TeacherDTO()))
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.StudyPeriodDates, f => new StudyPeriodDatesDto
        {
            StartDate = new DateOnly(2025, 9, 1),
            EndDate = new DateOnly(2026, 5, 31),
        });

    public static WorkshopBaseDto Generate() => Faker.Generate();

    public static List<WorkshopBaseDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopBaseDto dto) => Faker.Populate(dto);
}