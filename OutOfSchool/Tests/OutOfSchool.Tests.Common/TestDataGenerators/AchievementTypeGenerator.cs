using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class AchievementTypeGenerator
{
    private static readonly Faker<AchievementType> faker = new Faker<AchievementType>()
        .RuleFor(x => x.Id, f => f.Random.Long())
        .RuleFor(x => x.Title, f => f.Random.String())
        .RuleFor(x => x.TitleEn, f => f.Random.String());

    /// <summary>
    /// Generates new instance of the <see cref="AchievementType"/> class.
    /// </summary>
    /// <returns><see cref="AchievementType"/> object with random data.</returns>
    public static AchievementType Generate() => faker.Generate();
}
