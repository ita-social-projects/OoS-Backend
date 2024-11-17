using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Employee"/> objects.
/// </summary>
public static class EmployeesGenerator
{
    private static readonly Faker<Employee> faker = new Faker<Employee>()
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.IsDeleted, _ => false)
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.BlockingType, _ => BlockingType.None);

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="Employee"/> object.</returns>
    public static Employee Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Employee"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Employee> Generate(int count) => faker.Generate(count);
}
