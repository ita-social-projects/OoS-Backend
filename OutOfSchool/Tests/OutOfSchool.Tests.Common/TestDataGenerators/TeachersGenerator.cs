using System;
using System.Collections.Generic;
using System.Linq;

using Bogus;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    /// <summary>
    /// Contains methods to generate fake <see cref="Teacher"/> objects.
    /// </summary>
    public static class TeachersGenerator
    {
        private static readonly Faker<Teacher> faker = new Faker<Teacher>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
            .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
            .RuleFor(x => x.Image, f => f.Person.Avatar);

        /// <summary>
        /// Creates new instance of the <see cref="Teacher"/> class with random data.
        /// </summary>
        /// <returns><see cref="Teacher"/> object.</returns>
        public static Teacher Generate() => faker.Generate();

        /// <summary>
        /// Creates new instance of the <see cref="Teacher"/> class with random data and assigns given <paramref name="workshopId"/>.
        /// </summary>
        /// <param name="workshopId">The workshop id to assign to generated teacher.</param>
        /// <returns><see cref="Teacher"/> object.</returns>
        public static Teacher Generate(long workshopId) => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="Teacher"/> objects.
        /// </summary>
        /// <param name="number">Number of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Teacher"/> objects.</returns>
        public static List<Teacher> Generate(int number) => faker.Generate(number);

        /// <summary>
        /// Assigns given <paramref name="workshopId"/> to the given <paramref name="teacher"/>
        /// </summary>
        /// <returns><see cref="Teacher"/> object with assigned <paramref name="workshopId"/>.</returns>
        public static Teacher WithWorkshopId(this Teacher teacher, long workshopId)
        {
            _ = teacher ?? throw new ArgumentNullException(nameof(teacher));

            teacher.WorkshopId = workshopId;

            return teacher;
        }

        /// <summary>
        /// Assigns given <paramref name="workshopId"/> to the each item of the given <paramref name="teachers"/> collection
        /// </summary>
        /// <returns>Input collection with assigned <paramref name="workshopId"/>.</returns>
        public static List<Teacher> WithWorkshopId(this List<Teacher> teachers, long workshopId)
        {
            _ = teachers ?? throw new ArgumentNullException(nameof(teachers));
            teachers.ForEach(t => t.WithWorkshopId(workshopId));

            return teachers;
        }
    }
}
