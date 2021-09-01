using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    /// <summary>
    /// Contains methods to generate fake <see cref="Provider"/> objects.
    /// </summary>
    public static class ProvidersGenerator
    {
        private static readonly Faker<Provider> faker = new Faker<Provider>()
            .RuleFor(x => x.Id, f => f.UniqueIndex)
            .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
            .RuleFor(x => x.Website, f => f.Internet.Url())
            .RuleFor(x => x.Facebook, f => f.Internet.Url())
            .RuleFor(x => x.Instagram, f => f.Internet.Url())
            .RuleFor(x => x.Description, f => f.Company.CatchPhrase())
            .RuleFor(x => x.DirectorDateOfBirth, f => f.Person.DateOfBirth)
            .RuleFor(x => x.EdrpouIpn, f => f.Random.ReplaceNumbers("########"))
            .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
            .RuleFor(x => x.Founder, f => f.Person.FullName)
            .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
            .RuleFor(x => x.Type, f => f.Random.ArrayElement((ProviderType[])Enum.GetValues(typeof(ProviderType))))
            .RuleFor(x => x.Status, f => f.Random.Bool())
            .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
            .RuleFor(x => x.LegalAddress, _ => AddressGenerator.Generate())
            .RuleFor(x => x.ActualAddress, _ => AddressGenerator.Generate());

        /// <summary>
        /// Creates new instance of the <see cref="Provider"/> class with random data.
        /// </summary>
        /// <returns><see cref="Provider"/> object.</returns>
        public static Provider Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="Provider"/> objects.
        /// </summary>
        /// <param name="number">Number of instances to generate.</param>
        public static List<Provider> Generate(int number) => faker.Generate(number);
    }
}
