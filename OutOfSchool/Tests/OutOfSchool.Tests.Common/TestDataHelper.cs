using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bogus;
using OutOfSchool.Common.PermissionsModule;

namespace OutOfSchool.Tests.Common;

/// <summary>
/// Contains method to help with test data.
/// </summary>
public static class TestDataHelper
{
    private static readonly string[] EdrpouIpnFormats = { "########", "##########" };

    private static readonly Faker faker = new Faker();

    /// <summary>
    /// Gets random element from the collection.
    /// </summary>
    /// <typeparam name="T">Collection type.</typeparam>
    /// <param name="collection">Items to get element from.</param>
    /// <returns>Random element from the collection.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// When <paramref name="collection"/> is null.
    /// </exception>
    public static T RandomItem<T>(this ICollection<T> collection)
        => collection == null
            ? throw new ArgumentNullException(nameof(collection))
            : faker.Random.CollectionItem(collection);

    /// <summary>
    /// Gets the negative (int.MinValue .. -1) number.
    /// </summary>
    public static int GetNegativeInt() => faker.Random.Number(int.MinValue, -1);

    /// <summary>
    /// Gets the positive (1 .. int.MaxValue) number.
    /// </summary>
    public static int GetPositiveInt() => faker.Random.Number(1, int.MaxValue);

    /// <summary>
    /// Gets the positive (min .. max) number.
    /// </summary>
    public static int GetPositiveInt(int min,int max) => faker.Random.Number(min, max);


    /// <summary>
    /// Gets the positive (1 .. max) number.
    /// </summary>
    public static int GetPositiveInt(int max) => faker.Random.Number(1, max);

    /// <summary>
    /// Gets the collection of the positive (1 .. int.MaxValue) numbers.
    /// </summary>
    public static int[] GetPositiveInts(int count) => faker.Random.Digits(count, 1, 9);


    /// <summary>
    /// Applies an <paramref name="setter"/> on the given <paramref name="item"/>
    /// </summary>
    /// <typeparam name="TEntity">Item type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <param name="item">Item to apply setter on.</param>
    /// <param name="setter">Setter action to apply a value.</param>
    /// <param name="value">Value to apply.</param>
    /// <returns>Updated item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public static TEntity ApplyOnItem<TEntity, TValue>(TEntity item, Action<TEntity, TValue> setter, TValue value)
        where TEntity : class
    {
        _ = item ?? throw new ArgumentNullException(nameof(item));

        setter(item, value);

        return item;
    }

    /// <summary>
    /// Applies <paramref name="setter"/> on the given <paramref name="collection"/>
    /// </summary>
    /// <typeparam name="TEntity">Type of the collection element.</typeparam>
    /// <typeparam name="TValue">Type of the value to set.</typeparam>
    /// <param name="collection">Collection to set value on.</param>
    /// <param name="setter">Function which set value on the collection item.</param>
    /// <param name="value">Value to set.</param>
    /// <returns>Updated collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static List<TEntity> ApplyOnCollection<TEntity, TValue>(
        this List<TEntity> collection,
        Func<TEntity, TValue, TEntity> setter,
        TValue value)
        where TEntity : class
    {
        _ = collection ?? throw new ArgumentNullException(nameof(collection));

        collection.ForEach(item => setter(item, value));

        return collection;
    }

    /// <summary>
    /// Gets random Edrpou/Ipn string.
    /// </summary>
    public static string EdrpouIpnString =>
        Regex.Replace(faker.Random.ReplaceNumbers(faker.PickRandom(EdrpouIpnFormats)), "^0", "1");

    /// <summary>
    /// Gets random "job area" string to use as fake role name in our test cases
    /// </summary>
    public static string GetRandomRole() => faker.Name.JobArea();
    public static string GetRandomWords() => faker.Random.Words(3);

    public static string GetRandomEmail() => faker.Internet.Email();
    public static string GetFakePackedPermissions() =>
        faker.Random.ArrayElements((Permissions[])Enum.GetValues(typeof(Permissions)), 10).PackPermissionsIntoString();
}