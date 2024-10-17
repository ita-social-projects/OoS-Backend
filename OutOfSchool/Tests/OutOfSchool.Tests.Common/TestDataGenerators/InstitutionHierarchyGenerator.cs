using Bogus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;
using System;
using System.Linq;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class InstitutionHierarchyGenerator
{
    private static readonly Faker<InstitutionHierarchy> Faker = new Faker<InstitutionHierarchy>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.Directions, f => testDirection.Generate(3).ToList());

    private static readonly Faker<Direction> testDirection = new Faker<Direction>()
        .RuleFor(x => x.Id, f => f.Random.Long())
        .RuleFor(x => x.Title, f => f.Company.CompanyName());

    public static InstitutionHierarchy Generate() => Faker.Generate();
}
