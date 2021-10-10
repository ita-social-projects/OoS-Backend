using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ParentDtoGenerator
    {
        private static Faker<ParentDTO> faker = new Faker<ParentDTO>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.UserId, _ => Guid.NewGuid().ToString());

        public static ParentDTO Generate() => faker.Generate();

        public static List<ParentDTO> Generate(int count) => faker.Generate(count);

        public static ParentDTO WithUserId(this ParentDTO parentDto, string userId)
            => TestDataHelper.ApplyOnItem(parentDto, (x, y) => x.UserId = y, userId);
    }
}
