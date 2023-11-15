using System;
using System.Collections.Generic;

using Bogus;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class RatingsGenerator
{
    private static readonly Faker faker = new Faker();

    public static AverageRatingDto GetAverageRating(Guid id)
    {
        return new AverageRatingDto()
        {
            EntityId = id,
        };
    }

    public static IEnumerable<AverageRatingDto> GetAverageRatings(IEnumerable<Guid> ids)
    {
        var ratings = new List<AverageRatingDto>();
        
        foreach (var id in ids)
        {
            ratings.Add(GetAverageRating(id));
        }

        return ratings;
    }
}