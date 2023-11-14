using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class ChildUpdateDtoGenerator
{
    private static readonly Faker<ChildUpdateDto> faker = new Faker<ChildUpdateDto>()
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Person.LastName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.Gender, f => f.Random.Enum<Gender>())
        .RuleFor(x => x.PlaceOfStudy, f => f.Company.CompanyName());

    /// <summary>
    /// Creates new instance of the <see cref="ChildCreateDto"/> class with random data.
    /// </summary>
    /// <returns><see cref="ChildUpdateDto"/> object.</returns>
    public static ChildUpdateDto Generate() => faker.Generate();
}