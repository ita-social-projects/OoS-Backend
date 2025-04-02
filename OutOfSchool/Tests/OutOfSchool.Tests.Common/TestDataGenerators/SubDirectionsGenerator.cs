using Bogus;
using OutOfSchool.Services.Models;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class SubDirectionsGenerator
{
    private static readonly Faker<SubDirection> faker = new Faker<SubDirection>()
        .RuleFor(x => x.Id, f => f.Random.Long(min: 1, max: 1_000_000))
        .RuleFor(x => x.Title, f => f.Company.CompanyName());

    public static SubDirection Generate(long directionId) => faker.Generate().WithDirectionId(directionId);

    public static List<SubDirection> Generate(List<long> directionIds) =>
        directionIds.Select(id => Generate(id)).ToList();

    public static SubDirection WithDirectionId(this SubDirection subDirection, long directionId)
    {
        subDirection.DirectionId = directionId;
        return subDirection;
    }
}
