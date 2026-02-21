using Bogus;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Tag"/> objects.
/// </summary>
public static class TagsGenerator
{
    private static readonly Faker<Tag> faker = new Faker<Tag>()
        .RuleFor(x => x.Id, f => f.Random.Long(1, 100))
        .RuleFor(x => x.Name, f => f.Commerce.ProductName())
        .RuleFor(x => x.NameEn, f => f.Commerce.ProductName());

    /// <summary>
    /// Creates new instance of the <see cref="Tag"/> class with random data.
    /// </summary>
    /// <returns><see cref="Tag"/> object.</returns>
    public static Tag Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Tag"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Tag"/> objects.</returns>
    public static List<Tag> Generate(int count) => faker.Generate(count);

    public static void Populate(Tag tag) => faker.Populate(tag);

    public static List<Tag> WithWorkshop(this List<Tag> tags, Workshop workshop)
    {
        _ = tags ?? throw new ArgumentNullException(nameof(tags));
        tags.ForEach(t => t.WithWorkshopId(workshop));

        return tags;
    }

    /// <summary>
    /// Assigns given <paramref name="workshop"/> to the given <paramref name="tag"/>
    /// </summary>
    /// <returns><see cref="Tag"/> object with assigned <paramref name="workshop"/>.</returns>
    public static Tag WithWorkshopId(this Tag tag, Workshop workshop)
    {
        _ = tag ?? throw new ArgumentNullException(nameof(tag));
        if (tag.Workshops is null)
        {
            tag.Workshops = new List<Workshop>();
        }
        tag.Workshops.Add(workshop);

        return tag;
    }
}
