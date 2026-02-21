using Bogus;
using OutOfSchool.ElasticsearchData.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class CodeficatorAddressESGenerator
{
    static CodeficatorAddressESGenerator()
    {
        faker = new Faker<CodeficatorAddressES>()
            .RuleFor(x => x.Id, f => f.IndexFaker)
            .RuleFor(x => x.FullAddress, f => f.Address.FullAddress())
            .RuleFor(x => x.Category, f => f.Lorem.Word())
            .RuleFor(x => x.ParentId, f => f.Random.Long(1))
            .RuleFor(x => x.Parent, f => new CodeficatorAddressES())
            .RuleFor(x => x.Region, f => f.Address.State())
            .RuleFor(x => x.District, f => f.Lorem.Word())
            .RuleFor(x => x.TerritorialCommunity, f => f.Lorem.Word())
            .RuleFor(x => x.Settlement, f => f.Lorem.Word())
            .RuleFor(x => x.CityDistrict, f => f.Lorem.Word())
            .RuleFor(x => x.Latitude, f => f.Address.Latitude())
            .RuleFor(x => x.Longitude, f => f.Address.Longitude())
            .RuleFor(x => x.Order, f => f.Random.Number(1, 100))
            .RuleFor(x => x.FullName, f => f.Lorem.Sentence(3));

        // Increment initial value of IndexFaker to have first created entity with Id=1
        // and prevent System.InvalidOperationException when it is added to the DbContext
        (faker as IFakerTInternal).FakerHub.IndexFaker++;
    }

    private static readonly Faker<CodeficatorAddressES> faker;

    /// <summary>
    /// Generates new instance of the <see cref="AddressES"/> class.
    /// </summary>
    /// <returns><see cref="AddressES"/> object with random data.</returns>
    public static CodeficatorAddressES Generate() => faker.Generate();
}
