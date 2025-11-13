using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopBaseDtoGenerator
{
    public static readonly Faker<WorkshopBaseDto> Faker = new Faker<WorkshopBaseDto>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid), f => f.Random.Guid())
        .RuleForType(typeof(long), f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string), f => f.Lorem.Word())
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.AgeComposition, AgeComposition.SameAge)
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.ShortTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.NoAgeRestrictions, false)
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.IsPaid, true)
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 10000))
        .RuleFor(x => x.PayRate, f => f.PickRandom(PayRateType.Hour, PayRateType.Day, PayRateType.Month, PayRateType.Year, PayRateType.Course))
        .RuleFor(x => x.AvailableSeats, f => f.Random.UInt(5, 50))
        .RuleFor(x => x.AreThereBenefits, true)
        .RuleFor(x => x.PreferentialTermsOfParticipation, f => f.Lorem.Sentence())
        .RuleFor(x => x.CompetitiveSelection, true)
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence())
        .RuleFor(x => x.WorkshopDescriptionItems, f => WorkshopDescriptionItemDtoGenerator.Generate(6))
        .RuleFor(x => x.InstitutionId, f => f.Random.Guid())
        .RuleFor(x => x.Institution, f => f.Lorem.Word())
        .RuleFor(x => x.InstitutionHierarchyId, f => f.Random.Guid())
        .RuleFor(x => x.InstitutionHierarchy, f => f.Lorem.Word())
        .RuleFor(x => x.DirectionIds, _ => new List<long>())
        .RuleFor(x => x.SubDirectionIds, _ => new List<long>())
        .RuleFor(x => x.Keywords, f => f.Make(new Random().Next(5, 5), () => f.Lorem.Word()).Distinct())
        .RuleFor(x => x.Teachers, f => f.Make(new Random().Next(1, 3), () => new TeacherDTO()))
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ProviderLicenseStatus, f => f.PickRandom<ProviderLicenseStatus>())
        .RuleFor(x => x.StudyPeriodDates, f => new StudyPeriodDatesDto
        {
            StartDate = new DateOnly(2025, 9, 1),
            EndDate = new DateOnly(2025, 9, 1).AddMonths(10),
        });

    public static WorkshopBaseDto Generate() => Faker.Generate();

    public static List<WorkshopBaseDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopBaseDto dto) => Faker.Populate(dto);
}