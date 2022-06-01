using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ChildDtoGenerator
    {
        private static readonly Faker<ChildDto> faker = new Faker<ChildDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
            .RuleFor(x => x.Gender, f => f.Random.Enum<Gender>())
            .RuleFor(x => x.PlaceOfStudy, f => f.Company.CompanyName());

        /// <summary>
        /// Creates new instance of the <see cref="Child"/> class with random data.
        /// </summary>
        /// <returns><see cref="Child"/> object.</returns>
        public static ChildDto Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="Child"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Child"/> objects.</returns>
        public static List<ChildDto> Generate(int count) => faker.Generate(count);

        public static ChildDto WithParent(this ChildDto child, ParentDtoWithContactInfo parent)
        {
            _ = child ?? throw new ArgumentNullException(nameof(child));

            child.Parent = parent;
            child.ParentId = parent?.Id ?? default;

            return child;
        }

        public static List<ChildDto> WithParent(this List<ChildDto> children, ParentDtoWithContactInfo parent)
        {
            _ = children ?? throw new ArgumentNullException(nameof(children));

            children.ForEach(
                child =>
                {
                    child.Parent = parent;
                    child.ParentId = parent?.Id ?? default;
                });

            return children;
        }

        public static ChildDto WithSocial(this ChildDto child, SocialGroupDto socialGroup)
        {
            _ = child ?? throw new ArgumentNullException(nameof(child));

            child.SocialGroupId = socialGroup?.Id ?? default;

            return child;
        }

        public static List<ChildDto> WithSocial(this List<ChildDto> children, SocialGroupDto socialGroup)
        {
            _ = children ?? throw new ArgumentNullException(nameof(children));

            children.ForEach(child => child.WithSocial(socialGroup));

            return children;
        }
    }
}
