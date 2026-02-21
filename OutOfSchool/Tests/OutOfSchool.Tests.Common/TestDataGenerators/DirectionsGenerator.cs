using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class DirectionsGenerator
{
    private static readonly Faker<Direction> faker = new Faker<Direction>()
        .RuleFor(x => x.Id, f => f.Random.Long(min: 1, max: 1_000_000))
        .RuleFor(x => x.Title, f => f.Company.CompanyName());
    
    public static Direction Generate() => faker.Generate();
    
    public static List<Direction> Generate(int count) => faker.Generate(count);
    
    public static Direction WithId(this Direction direction, long id)
    {
        direction.Id = id;
        return direction;
    }
}