using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="ParentDtoWithContactInfo"/> objects.
/// </summary>
public static class ParentDtoWithContactInfoGenerator
{
    private static Faker<ParentDtoWithContactInfo> faker = new Faker<ParentDtoWithContactInfo>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.UserId, _ => Guid.NewGuid().ToString())
        .RuleFor(x => x.Email, _ => "fake@email.com");

    /// <summary>
    /// Generates new instance of the <see cref="ParentDtoWithContactInfo"/> class.
    /// </summary>
    /// <returns><see cref="ParentDtoWithContactInfo"/> object with random data.</returns>
    public static ParentDtoWithContactInfo Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ParentDtoWithContactInfo"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="ParentDtoWithContactInfo"/> objects.</returns>
    public static List<ParentDtoWithContactInfo> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Assigns given <paramref name="userId"/> to the given <paramref name="parentDtoWithContactInfo"/>
    /// </summary>
    /// <returns><see cref="ParentDtoWithContactInfo"/> object with assigned <paramref name="userId"/>.</returns>
    public static ParentDtoWithContactInfo WithUserId(this ParentDtoWithContactInfo parentDtoWithContactInfo, string userId)
        => TestDataHelper.ApplyOnItem(parentDtoWithContactInfo, (x, y) => x.UserId = y, userId);
}