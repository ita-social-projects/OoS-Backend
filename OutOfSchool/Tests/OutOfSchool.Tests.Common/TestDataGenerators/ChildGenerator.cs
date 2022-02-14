using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    /// <summary>
    /// Contains methods to generate fake <see cref="Child"/> objects.
    /// </summary>
    public static class ChildGenerator
    {
        private static readonly Faker<Child> faker = new Faker<Child>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
            .RuleFor(x => x.Gender, f => f.PickRandom<Gender>())
            .RuleFor(x => x.PlaceOfStudy, f => f.Company.CompanyName());

        /// <summary>
        /// Creates new instance of the <see cref="Child"/> class with random data.
        /// </summary>
        /// <returns><see cref="Child"/> object.</returns>
        public static Child Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="Child"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Child"/> objects.</returns>
        public static List<Child> Generate(int count) => faker.Generate(count);

        public static Child WithParent(this Child child, Parent parent)
        {
            _ = child ?? throw new ArgumentNullException(nameof(child));

            child.Parent = parent;
            child.ParentId = parent?.Id ?? default;

            return child;
        }

        public static List<Child> WithParent(this List<Child> children, Parent parent)
        {
            _ = children ?? throw new ArgumentNullException(nameof(children));

            children.ForEach(x => x.WithParent(parent));

            return children;
        }

        public static Child WithSocial(this Child child, SocialGroup socialGroup)
        {
            _ = child ?? throw new ArgumentNullException(nameof(child));

            child.SocialGroup = socialGroup;
            child.SocialGroupId = socialGroup?.Id ?? default;

            return child;
        }

        public static List<Child> WithSocial(this List<Child> children, SocialGroup socialGroup)
        {
            _ = children ?? throw new ArgumentNullException(nameof(children));

            children.ForEach(x => x.WithSocial(socialGroup));

            return children;
        }
    }
}
