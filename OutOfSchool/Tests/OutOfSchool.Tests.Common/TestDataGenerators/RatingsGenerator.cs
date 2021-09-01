using System;
using System.Collections.Generic;
using System.Linq;

using Bogus;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class RatingsGenerator
    {
        private static readonly Faker faker = new Faker();

        public static Tuple<float, int> GetAverageRatingForProvider()
            => new Tuple<float, int>(faker.Random.Float(), faker.Random.Int());

        public static Dictionary<long, Tuple<float, int>> GetAverageRatingForRange(IEnumerable<long> items)
            => items.ToDictionary(i => i, i => GetAverageRatingForProvider());
    }
}
