using Bogus;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Achievement"/> objects.
/// </summary>
public static class AchievementGenerator
{
    private static readonly Faker<Achievement> faker = new Faker<Achievement>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.AchievementDate, _ => DateTime.Now)
        .RuleFor(x => x.Workshop, _ => WorkshopGenerator.Generate())
        .RuleFor(x => x.AchievementType, _ => AchievementTypeGenerator.Generate());

    /// <summary>
    /// Generates new instance of the <see cref="Achievement"/> class.
    /// </summary>
    /// <returns><see cref="Achievement"/> object with random data.</returns>
    public static Achievement Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Achievement"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Achievement> Generate(int count) => faker.Generate(count);

    public static Achievement WithAchievementTeachers(this Achievement achievement)
    {
        achievement.Teachers = AchievementTeacherGenerator.Generate(new Random().Next(1, 4))
            .WithAchievement(achievement);
        
        return achievement;
    }

    public static List<Achievement> WithAchievementTeachers(this List<Achievement> achievements)
        => achievements.Select(x => x.WithAchievementTeachers()).ToList();
}
