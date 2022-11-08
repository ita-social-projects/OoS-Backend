using Bogus;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="ShortEntityDto"/> objects.
/// </summary>
public static class ShortEntityDtoGenerator
{
    private static readonly Faker<ShortEntityDto> faker = new Faker<ShortEntityDto>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Title, f => f.Lorem.Word());

    /// <summary>
    /// Creates new instance of the <see cref="ShortEntityDto"/> class with random data.
    /// </summary>
    /// <returns><see cref="ShortEntityDto"/> object.</returns>
    public static ShortEntityDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ShortEntityDto"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<ShortEntityDto> Generate(int count) => faker.Generate(count);

}
