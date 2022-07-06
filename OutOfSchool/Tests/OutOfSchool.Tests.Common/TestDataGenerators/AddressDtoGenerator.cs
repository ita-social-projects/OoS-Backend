using Bogus;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class AddressDtoGenerator
{
    private static readonly Faker<AddressDto> faker = new Faker<AddressDto>()
        .RuleFor(x => x.Id, f => f.IndexFaker)
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
    public static AddressDto Generate() => faker.Generate();
}