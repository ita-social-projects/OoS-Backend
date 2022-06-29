using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Application"/> objects.
/// </summary>
public static class ApplicationGenerator
{
    private static readonly TimeSpan timeShift = TimeSpan.FromDays(5);

    private static readonly Faker<Application> faker = new Faker<Application>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.CreationTime, f => f.Date.Between(DateTime.Now - timeShift, DateTime.Now + timeShift))
        .RuleFor(x => x.Status, f => f.Random.Enum<ApplicationStatus>());

    /// <summary>
    /// Generates new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <returns><see cref="Application"/> object with random data.</returns>
    public static Application Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Application"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Application"/> objects.</returns>
    public static List<Application> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the given <paramref name="application"/>
    /// </summary>
    /// <returns><see cref="Application"/> object with assigned <paramref name="workshopId"/>.</returns>
    public static Application WithWorkshop(this Application application, Workshop workshop)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Workshop = workshop;
        application.WorkshopId = workshop?.Id ?? default;

        return application;
    }

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the given <paramref name="applications"/>
    /// </summary>
    /// <returns><see cref="Application"/> object with assigned <paramref name="workshopId"/>.</returns>
    public static List<Application> WithWorkshop(this List<Application> applications, Workshop workshop)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(a => a.WithWorkshop(workshop));

        return applications;
    }

    public static Application WithParent(this Application application, Parent parent)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Parent = parent;
        application.ParentId = parent?.Id ?? default;

        return application;
    }

    public static List<Application> WithParent(this List<Application> applications, Parent parent)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(
            application =>
            {
                application.Parent = parent;
                application.ParentId = parent?.Id ?? default;
            });

        return applications;
    }

    public static Application WithChild(this Application application, Child child)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Child = child;
        application.ChildId = child?.Id ?? default;

        return application;
    }

    public static List<Application> WithChild(this List<Application> applications, Child child)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(
            application =>
            {
                application.Child = child;
                application.ChildId = child?.Id ?? default;
            });

        return applications;
    }
}
