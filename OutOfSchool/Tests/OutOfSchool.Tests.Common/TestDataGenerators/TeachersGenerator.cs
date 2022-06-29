using System;
using System.Collections.Generic;
using System.Linq;

using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Teacher"/> objects.
/// </summary>
public static class TeachersGenerator
{
    private static readonly Faker<Teacher> faker = new Faker<Teacher>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.FirstName, f => f.Person.FirstName)
        .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Person.LastName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.CoverImageId, f => f.Person.Avatar)
        .RuleFor(x => x.Gender, f => f.PickRandom<Gender>());

    /// <summary>
    /// Creates new instance of the <see cref="Teacher"/> class with random data.
    /// </summary>
    /// <returns><see cref="Teacher"/> object.</returns>
    public static Teacher Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Teacher"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Teacher"/> objects.</returns>
    public static List<Teacher> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the given <paramref name="teacher"/>
    /// </summary>
    /// <returns><see cref="Teacher"/> object with assigned <paramref name="workshop"/>.</returns>
    public static Teacher WithWorkshopId(this Teacher teacher, Workshop workshop)
    {
        _ = teacher ?? throw new ArgumentNullException(nameof(teacher));

        teacher.Workshop = workshop;
        teacher.WorkshopId = workshop.Id;

        return teacher;
    }

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the each item of the given <paramref name="teachers"/> collection
    /// </summary>
    /// <returns>Input collection with assigned <paramref name="workshop"/>.</returns>
    public static List<Teacher> WithWorkshop(this List<Teacher> teachers, Workshop workshop)
    {
        _ = teachers ?? throw new ArgumentNullException(nameof(teachers));
        teachers.ForEach(t => t.WithWorkshopId(workshop));

        return teachers;
    }
}