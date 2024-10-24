﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Provider"/> objects.
/// </summary>
public static class ProvidersGenerator
{
    private static readonly Faker<Provider> faker = new Faker<Provider>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
        .RuleFor(x => x.Website, f => f.Internet.Url())
        .RuleFor(x => x.Facebook, f => f.Internet.Url())
        .RuleFor(x => x.Instagram, f => f.Internet.Url())
        .RuleFor(x => x.DirectorDateOfBirth, f => f.Person.DateOfBirth)
        .RuleFor(x => x.EdrpouIpn, _ => TestDataHelper.EdrpouIpnString)
        .RuleFor(x => x.Email, _ => TestDataHelper.GetRandomEmail())
        .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
        .RuleFor(x => x.Founder, f => f.Person.FullName)
        .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
        .RuleFor(x => x.TypeId, _ => 1)
        .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
        .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>())
        .RuleFor(x => x.IsBlocked, _ => false)
        .RuleFor(x => x.UpdatedAt, _ => DateTime.Now);

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="Provider"/> object.</returns>
    public static Provider Generate() => faker.Generate().WithAddress();

    /// <summary>
    /// Generates a list of the <see cref="Provider"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Provider> Generate(int count) => faker.Generate(count).WithAddress();

    public static Provider WithAddress(this Provider provider)
    {
        var address = AddressGenerator.Generate();
        provider = TestDataHelper.ApplyOnItem(provider, (provider, address) => { provider.LegalAddress = address; provider.LegalAddressId = address.Id; }, address);
        address = AddressGenerator.Generate();
        return TestDataHelper.ApplyOnItem(provider, (provider, address) => { provider.ActualAddress = address; provider.ActualAddressId = address.Id; }, address);
    }

    public static List<Provider> WithAddress(this List<Provider> workshops)
        => workshops.Select(x => WithAddress(x)).ToList();

    public static Provider WithWorkshops(this Provider provider)
    {
        provider.Workshops = WorkshopGenerator.Generate(new Random().Next(1, 4))
            .WithProvider(provider);
        return provider;
    }

    public static List<Provider> WithWorkshops(this List<Provider> providers)
        => providers.Select(x => x.WithWorkshops()).ToList();

    public static Provider WithInstitutionId(this Provider provider, Guid institutionId)
    {
        provider.InstitutionId = institutionId;
        return provider;
    }

    public static List<Provider> WithInstitutionId(this List<Provider> providers, Guid institutionId, params int[] itemIndexes)
    {
        if (itemIndexes.Length == 0)
        {
            providers.ForEach(p => p.WithInstitutionId(institutionId));
        }
        else
        {
            foreach (var index in itemIndexes)
            {
                if (index < providers.Count)
                {
                    providers[index].WithInstitutionId(institutionId);
                }
            }
        }
        return providers;
    }

    public static Provider WithUser(this Provider provider)
    {
        provider.User = UserGenerator.Generate();
        provider.UserId = provider.User.Id;
        return provider;
    }

    public static List<Provider> WithUser(this List<Provider> providers)
        => providers.Select(x => x.WithUser()).ToList();
}
