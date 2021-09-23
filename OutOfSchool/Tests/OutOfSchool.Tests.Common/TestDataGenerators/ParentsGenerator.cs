using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ParentsGenerator
    {
        private static Faker<Parent> faker = new Faker<Parent>()
            .RuleFor(x => x.Id, f => f.UniqueIndex);

        public static Parent Generate()
        {
            Parent parent = faker.Generate();
            User user = UsersGenerator.Generate();
            parent.UserId = user.Id;
            parent.User = user;

            return parent;
        }

        public static List<Parent> Generate(int number)
        {
            List<Parent> parents = faker.Generate(number);
            foreach (Parent parent in parents)
            {
                User user = UsersGenerator.Generate();
                parent.UserId = user.Id;
                parent.User = user;
            }

            return parents;
        }
    }
}
