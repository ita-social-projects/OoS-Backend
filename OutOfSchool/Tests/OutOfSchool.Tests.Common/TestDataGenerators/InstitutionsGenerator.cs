using Bogus;
using OutOfSchool.Services.Models.SubordinationStructure;
using System;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Institution"/> objects.
/// </summary>
public static class InstitutionsGenerator
{
    private static readonly Faker<Institution> faker = new Faker<Institution>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.NumberOfHierarchyLevels, f => f.Random.Int())
        .RuleFor(x => x.Title, f => f.Company.CompanySuffix());

    /// <summary>
    /// Generates new instance of the <see cref="Institution"/> class.
    /// </summary>
    /// <returns><see cref="Institution"/> object with random data.</returns>
    public static Institution Generate() => faker.Generate();
}