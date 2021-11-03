using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    /// <summary>
    /// Contains methods to generate fake <see cref="ParentDTO"/> objects.
    /// </summary>
    public static class ParentDtoGenerator
    {
        private static Faker<ParentDTO> faker = new Faker<ParentDTO>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.UserId, _ => Guid.NewGuid().ToString());

        /// <summary>
        /// Generates new instance of the <see cref="ParentDTO"/> class.
        /// </summary>
        /// <returns><see cref="ParentDTO"/> object with random data.</returns>
        public static ParentDTO Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="ParentDTO"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ParentDTO"/> objects.</returns>
        public static List<ParentDTO> Generate(int count) => faker.Generate(count);

        /// <summary>
        /// Assigns given <paramref name="userId"/> to the given <paramref name="parentDto"/>
        /// </summary>
        /// <returns><see cref="ParentDTO"/> object with assigned <paramref name="userId"/>.</returns>
        public static ParentDTO WithUserId(this ParentDTO parentDto, string userId)
            => TestDataHelper.ApplyOnItem(parentDto, (x, y) => x.UserId = y, userId);
    }
}
