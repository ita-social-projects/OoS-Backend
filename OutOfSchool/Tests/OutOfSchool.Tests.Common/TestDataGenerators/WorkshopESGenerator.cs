﻿using System;
using Bogus;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class WorkshopESGenerator
{
    private static readonly Faker<WorkshopES> Faker = new Faker<WorkshopES>()
        .RuleForType(typeof(int), f => f.Random.Int())
        .RuleForType(typeof(Guid), f => f.Random.Guid())
        .RuleForType(typeof(long), f => f.Random.Long(0, long.MaxValue))
        .RuleForType(typeof(string), f => f.Lorem.Word())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.MinAge, f => f.Random.Number(1, 15))
        .RuleFor(x => x.MaxAge, f => f.Random.Number(16, 18))
        .RuleFor(x => x.Price, f => f.Random.Decimal(0, 100))
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.Keywords, f => f.Lorem.Sentence())
        .RuleFor(x => x.Address, f => AddressESGenerator.Generate());
        
    public static WorkshopES Generate() => Faker.Generate();
}