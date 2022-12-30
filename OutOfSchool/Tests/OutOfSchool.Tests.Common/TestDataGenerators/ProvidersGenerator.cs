using System;
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
        .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
        .RuleFor(x => x.Founder, f => f.Person.FullName)
        .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
        .RuleFor(x => x.TypeId, _ => 1)
        .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
        .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.LegalAddress, _ => AddressGenerator.Generate())
        .RuleFor(x => x.ActualAddress, _ => AddressGenerator.Generate())
        .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>())
        .RuleFor(x => x.IsBlocked, _ => false);

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="Provider"/> object.</returns>
    public static Provider Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Provider"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<Provider> Generate(int count) => faker.Generate(count);

    public static Provider WithWorkshops(this Provider provider)
    {
        provider.Workshops = WorkshopGenerator.Generate(new Random().Next(1, 4))
            .WithProvider(provider);
        return provider;
    }

    public static List<Provider> WithWorkshops(this List<Provider> providers)
        => providers.Select(x => x.WithWorkshops()).ToList();
}
