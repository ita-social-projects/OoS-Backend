using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class UsersGenerator
    {
        private static readonly Faker<User> faker = new Faker<User>()
            .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName);

        public static User Generate() => faker.Generate();

        public static List<User> Generate(int number) => faker.Generate(number);
    }
}
