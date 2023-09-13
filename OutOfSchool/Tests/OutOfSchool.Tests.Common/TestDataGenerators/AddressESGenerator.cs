using Bogus;
using Nest;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class AddressESGenerator
{
    static AddressESGenerator()
    {
        faker = new Faker<AddressES>()
            .RuleFor(x => x.Id, f => f.IndexFaker)
            .RuleFor(x => x.City, f => f.Address.City())
            .RuleFor(x => x.Latitude, f => f.Address.Latitude())
            .RuleFor(x => x.Longitude, f => f.Address.Longitude())
            .RuleFor(x => x.CATOTTGId, 4970)
            .RuleFor(x => x.CodeficatorAddressES, f => CodeficatorAddressESGenerator.Generate())
            .RuleFor(x => x.Street, f => f.Address.StreetName())
            .RuleFor(x => x.BuildingNumber, f => f.Address.BuildingNumber())
            .RuleFor(x => x.Point, f => GeoLocation.TryCreate(f.Address.Latitude(), f.Address.Longitude()));

        // Increment initial value of IndexFaker to have first created entity with Id=1
        // and prevent System.InvalidOperationException when it is added to the DbContext
        (faker as IFakerTInternal).FakerHub.IndexFaker++;
    }

    private static readonly Faker<AddressES> faker;

    /// <summary>
    /// Generates new instance of the <see cref="AddressES"/> class.
    /// </summary>
    /// <returns><see cref="AddressES"/> object with random data.</returns>
    public static AddressES Generate() => faker.Generate();
}

