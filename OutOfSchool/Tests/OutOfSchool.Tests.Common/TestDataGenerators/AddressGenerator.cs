using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="Address"/> objects.
/// </summary>
public static class AddressGenerator
{
    static AddressGenerator()
    {
        faker = new Faker<Address>()
            .RuleFor(x => x.Id, f => f.IndexFaker)
            .RuleFor(x => x.CATOTTGId, 4970)
            .RuleFor(x => x.CATOTTG, _ => CATOTTGGenerator.Generate())
            .RuleFor(x => x.Street, f => f.Address.StreetName())
            .RuleFor(x => x.BuildingNumber, f => f.Address.BuildingNumber())
            .RuleFor(x => x.Latitude, f => f.Address.Latitude())
            .RuleFor(x => x.Longitude, f => f.Address.Longitude());

        // Increment initial value of IndexFaker to have first created entity with Id=1
        // and prevent System.InvalidOperationException when it is added to the DbContext
        (faker as IFakerTInternal).FakerHub.IndexFaker++;
    }

    private static readonly Faker<Address> faker;

    /// <summary>
    /// Generates new instance of the <see cref="Address"/> class.
    /// </summary>
    /// <returns><see cref="Address"/> object with random data.</returns>
    public static Address Generate() => faker.Generate();
}
