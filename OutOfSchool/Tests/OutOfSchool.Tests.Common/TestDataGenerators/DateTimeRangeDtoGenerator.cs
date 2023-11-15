using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class DateTimeRangeDtoGenerator
{
    private static readonly Faker<DateTimeRangeDto> Faker = new Faker<DateTimeRangeDto>()
        .RuleFor(x => x.Id, f => f.Random.Long(0))
        .RuleFor(x => x.StartTime, f => new TimeSpan())
        .RuleFor(x => x.EndTime, f => new TimeSpan())
        .RuleFor(x => x.Workdays, _ => new List<DaysBitMask>());

    public static DateTimeRangeDto Generate() => Faker.Generate();

    public static List<DateTimeRangeDto> Generate(int count) => Faker.Generate(count);
}