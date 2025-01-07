using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Official"/> objects.
/// </summary>
public static class OfficialGenerator
{
    private static readonly Faker<Official> faker = new Faker<Official>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.UpdatedAt, _ => DateTime.Now)
        .RuleFor(x => x.ActiveFrom, (f, w) => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DateOnly.FromDateTime(DateTime.Now)))
        .RuleFor(x => x.ActiveTo, f => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(300))))
        .RuleFor(x => x.IndividualId, _ => Guid.NewGuid())
        .RuleFor(x => x.PositionId, _ => Guid.NewGuid());

    /// <summary>
    /// Creates new instance of the <see cref="Official"/> class with random data.
    /// </summary>
    /// <returns><see cref="Official"/> object.</returns>
    public static Official Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Official"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Official> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Populates an existing instance of the <see cref="Official"/> class with random data.
    /// </summary>
    public static void Populate(Official dto) => faker.Populate(dto);
}