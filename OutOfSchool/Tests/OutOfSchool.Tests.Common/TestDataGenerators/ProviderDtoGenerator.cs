﻿using System;
using System.Collections.Generic;

using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class ProviderDtoGenerator
{
    private static readonly Faker<ProviderDto> faker = new Faker<ProviderDto>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
        .RuleFor(x => x.Website, f => f.Internet.Url())
        .RuleFor(x => x.Facebook, f => f.Internet.Url())
        .RuleFor(x => x.Instagram, f => f.Internet.Url())
        .RuleFor(x => x.DirectorDateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.EdrpouIpn, _ => TestDataHelper.EdrpouIpnString)
        .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
        .RuleFor(x => x.Founder, f => f.Person.FullName)
        .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
        .RuleFor(x => x.Type, _ => new ProviderTypeDto{Id=1, Name = "pro"})
        .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
        .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.LegalAddress, _ => AddressDtoGenerator.Generate())
        .RuleFor(x => x.ActualAddress, _ => AddressDtoGenerator.Generate())
        .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>());

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="Provider"/> object.</returns>
    public static ProviderDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Provider"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<ProviderDto> Generate(int count) => faker.Generate(count);

    public static ProviderDto WithUserId(this ProviderDto providerDto, string userId)
        => TestDataHelper.ApplyOnItem(providerDto, (x, y) => x.UserId = y, userId);

}