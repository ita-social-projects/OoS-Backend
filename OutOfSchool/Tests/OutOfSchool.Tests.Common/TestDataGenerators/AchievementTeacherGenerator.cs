using Bogus;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class AchievementTeacherGenerator
{
    private static readonly Faker<AchievementTeacher> faker = new Faker<AchievementTeacher>()
        .RuleFor(x => x.Id, f => f.Random.Long())
        .RuleFor(x => x.Title, f => f.Name.FullName());

    /// <summary>
    /// Generates new instance of the <see cref="AchievementTeacher"/> class.
    /// </summary>
    /// <returns><see cref="AchievementTeacher"/> object with random data.</returns>
    public static AchievementTeacher Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="AchievementTeacher"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<AchievementTeacher> Generate(int count) => faker.Generate(count);

    public static AchievementTeacher WithAchievement(this AchievementTeacher achievementTeacher, Achievement achievement)
    {
        _ = achievementTeacher ?? throw new ArgumentNullException(nameof(achievementTeacher));

        achievementTeacher.Achievement = achievement;
        achievementTeacher.AchievementId = achievement?.Id ?? default;

        return achievementTeacher;
    }

    public static List<AchievementTeacher> WithAchievement(this List<AchievementTeacher> achievementTeachers, Achievement achievement)
    {
        _ = achievementTeachers ?? throw new ArgumentNullException(nameof(achievementTeachers));

        achievementTeachers.ForEach(x => x.WithAchievement(achievement));

        return achievementTeachers;
    }
}
