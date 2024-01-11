using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="ProviderAdmin"/> objects.
/// </summary>
public static class ProviderAdminsGenerator
{
    private static readonly Faker<ProviderAdmin> faker = new Faker<ProviderAdmin>()
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.IsDeleted, _ => false)
        .RuleFor(x => x.ProviderId, f => f.Random.Guid())
        .RuleFor(x => x.IsDeputy, _ => false)
        .RuleFor(x => x.BlockingType, _ => BlockingType.None);

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="ProviderAdmin"/> object.</returns>
    public static ProviderAdmin Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ProviderAdmin"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<ProviderAdmin> Generate(int count) => faker.Generate(count);
}
