using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
internal class CATOTTGGenerator
{
    static CATOTTGGenerator()
    {
        faker = new Faker<CATOTTG>()
            .RuleFor(x => x.Id, f => f.IndexFaker)
            .RuleFor(x => x.Name, "Нікополь")
            .RuleFor(x => x.Code, "UA12080050010010114");

        // Increment initial value of IndexFaker to have first created entity with Id=1
        // and prevent System.InvalidOperationException when it is added to the DbContext
        (faker as IFakerTInternal).FakerHub.IndexFaker++;
    }

    private static readonly Faker<CATOTTG> faker;

    /// <summary>
    /// Generates new instance of the <see cref="CATOTTG"/> class.
    /// </summary>
    /// <returns><see cref="CATOTTG"/> object with random data.</returns>
    public static CATOTTG Generate() => faker.Generate();
}
