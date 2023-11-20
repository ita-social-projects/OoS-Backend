using System;
using System.Collections.Generic;

using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Parent"/> objects.
/// </summary>
public static class ParentGenerator
{
    private static Faker<Parent> faker = new Faker<Parent>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.UserId, _ => Guid.NewGuid().ToString())
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.Gender, _ => Gender.Male);

    /// <summary>
    /// Generates new instance of the <see cref="ParentDTO"/> class.
    /// </summary>
    /// <returns><see cref="Parent"/> object with random data.</returns>
    public static Parent Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Parent"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Parent"/> objects.</returns>
    public static List<Parent> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Assigns given <paramref name="userId"/> to the given <paramref name="parent"/>
    /// </summary>
    /// <returns><see cref="Parent"/> object with assigned <paramref name="userId"/>.</returns>
    public static Parent WithUserId(this Parent parent, string userId)
        => TestDataHelper.ApplyOnItem(parent, (x, y) => x.UserId = y, userId);
}