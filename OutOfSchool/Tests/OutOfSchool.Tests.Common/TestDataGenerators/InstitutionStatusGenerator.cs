using Bogus;
using OutOfSchool.Services.Models;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="InstitutionStatus"/> objects.
/// </summary>
public class InstitutionStatusGenerator
{

    private static Faker<InstitutionStatus> faker = new Faker<InstitutionStatus>()
        .RuleFor(x => x.Id, f => f.IndexVariable++)
        .RuleFor(x => x.Name, f => f.Name.JobDescriptor())
        .RuleFor(x => x.Providers, ProvidersGenerator.Generate(3));

    /// <summary>
    /// Generates new instance of the <see cref="InstitutionStatus"/> class.
    /// </summary>
    /// <returns><see cref="InstitutionStatus"/> object with random data.</returns>
    public static InstitutionStatus Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="InstitutionStatus"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="InstitutionStatus"/> objects.</returns>
    public static List<InstitutionStatus> Generate(int count) => faker.Generate(count);
}