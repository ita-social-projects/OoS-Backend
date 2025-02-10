using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Position"/> objects.
/// </summary>
public static class PositionGenerator
{
    private static readonly Faker<Position> faker = new Faker<Position>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.UpdatedAt, _ => DateTime.Now)
        .RuleFor(x => x.ActiveFrom, (f, w) => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DateOnly.FromDateTime(DateTime.Now)))
        .RuleFor(x => x.ActiveTo, f => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(300))))
        .RuleFor(x => x.FullName, f => f.Music.Genre());

    /// <summary>
    /// Creates new instance of the <see cref="Position"/> class with random data.
    /// </summary>
    /// <returns><see cref="Position"/> object.</returns>
    public static Position Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Position"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Position> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Populates an existing instance of the <see cref="Position"/> class with random data.
    /// </summary>
    public static void Populate(Position dto) => faker.Populate(dto);
}