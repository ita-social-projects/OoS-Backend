using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.SocialGroup;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class ChildCreateDtoGenerator
{
    private static readonly Faker<ChildCreateDto> faker = new Faker<ChildCreateDto>()
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Person.LastName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.Gender, f => f.Random.Enum<Gender>())
        .RuleFor(x => x.PlaceOfStudy, f => f.Company.CompanyName());

    /// <summary>
    /// Creates new instance of the <see cref="ChildCreateDto"/> class with random data.
    /// </summary>
    /// <returns><see cref="ChildCreateDto"/> object.</returns>
    public static ChildCreateDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ChildCreateDto"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="ChildCreateDto"/> objects.</returns>
    public static List<ChildCreateDto> Generate(int count) => faker.Generate(count);
}