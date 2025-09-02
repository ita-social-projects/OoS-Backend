using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class StudyPeriodDatesDtoGenerator
{
    public static readonly Faker<StudyPeriodDatesDto> Faker = new Faker<StudyPeriodDatesDto>()
        .CustomInstantiator(f =>
        {
            var startDate = f.Date.RecentDateOnly();
            var endDate = f.Date.BetweenDateOnly(startDate, startDate.AddMonths(9));
            return new StudyPeriodDatesDto { StartDate = startDate, EndDate = endDate };
        });


    public static StudyPeriodDatesDto Generate() => Faker.Generate();

    public static List<StudyPeriodDatesDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(StudyPeriodDatesDto dto) => Faker.Populate(dto);
}