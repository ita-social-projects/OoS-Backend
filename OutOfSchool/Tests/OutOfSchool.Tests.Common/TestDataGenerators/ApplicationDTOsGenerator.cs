using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="ApplicationDto"/> objects.
/// </summary>
public static class ApplicationDTOsGenerator
{
    private static readonly TimeSpan timeShift = TimeSpan.FromDays(5);

    private static readonly Faker<ApplicationDto> faker = new Faker<ApplicationDto>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.CreationTime, f => f.Date.Between(DateTime.Now - timeShift, DateTime.Now + timeShift))
        .RuleFor(x => x.Status, f => ApplicationStatus.Approved);

    /// <summary>
    /// Generates new instance of the <see cref="ApplicationDto"/> class.
    /// </summary>
    /// <returns><see cref="ApplicationDto"/> object with random data.</returns>
    public static ApplicationDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ApplicationDto"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="ApplicationDto"/> objects.</returns>
    public static List<ApplicationDto> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the given <paramref name="application"/>
    /// </summary>
    /// <returns><see cref="ApplicationDto"/> object with assigned <paramref name="workshopId"/>.</returns>
    public static ApplicationDto WithWorkshopCard(this ApplicationDto application, WorkshopCard workshopCard)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Workshop = workshopCard;

        return application;
    }

    /// <summary>
    /// Assigns given <paramref name="workshopCard"/> to the given <paramref name="applications"/>
    /// </summary>
    /// <returns><see cref="ApplicationDto"/> object with assigned <paramref name="workshopId"/>.</returns>
    public static List<ApplicationDto> WithWorkshopCard(this List<ApplicationDto> applications, WorkshopCard workshopCard)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(a => a.WithWorkshopCard(workshopCard));

        return applications;
    }

    public static ApplicationDto WithParent(this ApplicationDto application, ParentDTO parent)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Parent = parent;
        application.ParentId = parent?.Id ?? Guid.Empty;

        return application;
    }

    public static List<ApplicationDto> WithParent(this List<ApplicationDto> applications, ParentDTO parent)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(
            application =>
            {
                application.Parent = parent;
                application.ParentId = parent?.Id ?? Guid.Empty;
            });

        return applications;
    }

    public static ApplicationDto WithChild(this ApplicationDto application, ChildDto child)
    {
        _ = application ?? throw new ArgumentNullException(nameof(application));

        application.Child = child;
        application.ChildId = child?.Id ?? Guid.Empty;

        return application;
    }

    public static List<ApplicationDto> WithChild(this List<ApplicationDto> applications, ChildDto child)
    {
        _ = applications ?? throw new ArgumentNullException(nameof(applications));

        applications.ForEach(
            application =>
            {
                application.Child = child;
                application.ChildId = child?.Id ?? Guid.Empty;
            });

        return applications;
    }
}