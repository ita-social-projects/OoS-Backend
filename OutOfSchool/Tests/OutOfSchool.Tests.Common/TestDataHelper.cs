using System;
using System.Collections.Generic;

using Bogus;

namespace OutOfSchool.Tests.Common
{
    /// <summary>
    /// Contains method to help with test data.
    /// </summary>
    public static class TestDataHelper
    {
        private static Faker faker = new Faker();

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

        public static TEntity ApplyOnItem<TEntity, TValue>(TEntity item, Action<TEntity, TValue> setter, TValue value)
            where TEntity : class
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            setter(item, value);

            return item;
        }

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
    }
}
