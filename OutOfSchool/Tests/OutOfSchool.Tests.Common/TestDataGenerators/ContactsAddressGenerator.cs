using Bogus;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class ContactsAddressGenerator
{
    static ContactsAddressGenerator()
    {
        faker = new Faker<ContactsAddress>()
            .RuleFor(x => x.CATOTTG, _ => CATOTTGGenerator.Generate())
            .RuleFor(x => x.CATOTTGId, (f, address) => address.CATOTTG.Id)
            .RuleFor(x => x.Street, f => f.Address.StreetName())
            .RuleFor(x => x.BuildingNumber, f => f.Address.BuildingNumber())
            .RuleFor(x => x.Latitude, f => f.Address.Latitude())
            .RuleFor(x => x.Longitude, f => f.Address.Longitude());
    }

    private static readonly Faker<ContactsAddress> faker;

    /// <summary>
    /// Generates new instance of the <see cref="ContactsAddress"/> class.
    /// </summary>
    /// <returns><see cref="ContactsAddress"/> object with random data.</returns>
    public static ContactsAddress Generate() => faker.Generate();
}