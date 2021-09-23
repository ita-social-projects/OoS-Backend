using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class RatingsGenerator
    {
        private static readonly Faker faker = new Faker();

        private static readonly Faker<Rating> fakerRating = new Faker<Rating>()
            .RuleFor(x => x.Id, f => f.UniqueIndex)
            .RuleFor(x => x.Rate, f => f.Random.Number(1, 5))
            .RuleFor(x => x.Type, f => f.PickRandom<RatingType>())
            .RuleFor(x => x.EntityId, f => f.Random.Long(1, long.MaxValue))
            .RuleFor(x => x.CreationTime, f => f.Date.Recent());

        public static Tuple<float, int> GetAverageRatingForProvider()
            => new Tuple<float, int>(faker.Random.Float(), faker.Random.Int());

        public static Dictionary<long, Tuple<float, int>> GetAverageRatingForRange(IEnumerable<long> items)
            => items.ToDictionary(i => i, i => GetAverageRatingForProvider());

        public static Rating Generate()
        {
            Rating rating = fakerRating.Generate();
            Parent parent = ParentsGenerator.Generate();
            rating.ParentId = parent.Id;
            rating.Parent = parent;
            return rating;
        }

        public static List<Rating> Generate(int number)
        {
            List<Rating> ratings = fakerRating.Generate(number);
            foreach(var rating in ratings)
            {
                Parent parent = ParentsGenerator.Generate();
                rating.ParentId = parent.Id;
                rating.Parent = parent;
            }
            return ratings;
        }
    }
}
