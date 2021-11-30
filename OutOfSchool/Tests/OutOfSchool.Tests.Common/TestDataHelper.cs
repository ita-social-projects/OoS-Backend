using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bogus;

namespace OutOfSchool.Tests.Common
{
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
        /// Gets the positive (1 .. int.MaxValue) number.
        /// </summary>
        public static int GetPositiveInt() => faker.Random.Number(1, int.MaxValue);

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
        /// <param name="item">Item to apply setter on.</param>
        /// <param name="setter">Setter action to apply a value.</param>
        /// <returns>Updated item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
        public static TEntity ApplyOnItem<TEntity>(this TEntity item, Action<TEntity> setter)
            where TEntity : class
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            setter(item);

            return item;
        }

        /// <summary>
        /// Applies <paramref name="setter"/> on the given <paramref name="collection"/>
        /// </summary>
        /// <typeparam name="TEntity">Type of the collection element.</typeparam>
        /// <param name="collection">Collection to set value on.</param>
        /// <param name="setter">Function which set value on the collection item.</param>
        /// <returns>Updated collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
        public static List<TEntity> ApplyOnCollection<TEntity>(
            this List<TEntity> collection,
            Action<TEntity> setter)
            where TEntity : class
        {
            _ = collection ?? throw new ArgumentNullException(nameof(collection));

            collection.ForEach(setter);

            return collection;
        }

        /// <summary>
        /// Gets random Edrpou/Ipn number.
        /// </summary>
        public static long EdrpouIpnNumber => long.Parse(faker.Random.ReplaceNumbers(faker.PickRandom(EdrpouIpnFormats)));

        /// <summary>
        /// Gets random Edrpou/Ipn string.
        /// </summary>
        public static string EdrpouIpnString => faker.Random.ReplaceNumbers(faker.PickRandom(EdrpouIpnFormats));
    }
}
