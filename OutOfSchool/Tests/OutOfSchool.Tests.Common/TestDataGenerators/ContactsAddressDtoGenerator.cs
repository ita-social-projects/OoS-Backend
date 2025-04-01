using Bogus;
using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class ContactsAddressDtoGenerator
{
    static ContactsAddressDtoGenerator()
    {
        faker = new Faker<ContactsAddressDto>()
            .RuleFor(x => x.CATOTTGId, (f, address) => CATOTTGGenerator.Generate().Id)
            .RuleFor(x => x.Street, f => f.Address.StreetName())
            .RuleFor(x => x.BuildingNumber, f => f.Address.BuildingNumber())
            .RuleFor(x => x.Latitude, f => f.Address.Latitude())
            .RuleFor(x => x.Longitude, f => f.Address.Longitude());
    }

    private static readonly Faker<ContactsAddressDto> faker;

    /// <summary>
    /// Generates new instance of the <see cref="ContactsAddressDto"/> class.
    /// </summary>
    /// <returns><see cref="ContactsAddressDto"/> object with random data.</returns>
    public static ContactsAddressDto Generate() => faker.Generate();
}