
using Bogus;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Address"/> objects.
/// </summary>
public static class AddressGenerator
{
    private static readonly Faker<Address> faker = new Faker<Address>()
        .RuleFor(x => x.Id, f => f.IndexFaker++)
        .RuleFor(x => x.Region, f => f.Address.State())
        .RuleFor(x => x.District, f => f.Address.County())
        .RuleFor(x => x.City, f => f.Address.City())
        .RuleFor(x => x.Street, f => f.Address.StreetName())
        .RuleFor(x => x.BuildingNumber, f => f.Address.BuildingNumber())
        .RuleFor(x => x.Latitude, f => f.Address.Latitude())
        .RuleFor(x => x.Longitude, f => f.Address.Longitude());

    /// <summary>
    /// Generates new instance of the <see cref="Address"/> class.
    /// </summary>
    /// <returns><see cref="Address"/> object with random data.</returns>
    public static Address Generate() => faker.Generate();
}