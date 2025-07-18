using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;


namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class DateTimeRangeGenerator
{
    private static readonly Faker<DateTimeRange> Faker = new Faker<DateTimeRange>()
        .RuleFor(x => x.Id, f => f.Random.Long(0))
        .RuleFor(x => x.WorkshopId, f => f.Random.Uuid())
        .RuleFor(x => x.StartTime, f => new TimeSpan())
        .RuleFor(x => x.EndTime, f => new TimeSpan())
        .RuleFor(x => x.Workdays, _ => new DaysBitMask());

    public static DateTimeRange Generate() => Faker.Generate();

    public static List<DateTimeRange> Generate(int count) => Faker.Generate(count);
}