using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class WorkshopGenerator
    {
        private static Faker<Workshop> faker = new Faker<Workshop>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Phone, f => f.Phone.ToString())
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.Website, f => f.Internet.Url())
            .RuleFor(x => x.Facebook, f => f.Internet.Url())
            .RuleFor(x => x.Instagram, f => f.Internet.Url())
            .RuleFor(x => x.MinAge, f => f.Random.Number(1, 18))
            //            .RuleFor(x => x.MaxAge, f => f.)
            .RuleFor(x => x.Price, f => f.Random.Decimal())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
            .RuleFor(x => x.DisabilityOptionsDesc, f => f.Lorem.Sentence())
            .RuleFor(x => x.Logo, f => f.Image.LoremFlickrUrl())
            .RuleFor(x => x.Head, f => f.Person.FullName)
            .RuleFor(x => x.HeadDateOfBirth, f => f.Person.DateOfBirth)
            .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
            .RuleFor(x => x.Keywords, f => f.Lorem.Sentence())
            .RuleFor(x => x.IsPerMonth, f => f.Random.Bool());
        //.RuleFor(x => x.DepartmentId, f => f.)
        //.RuleFor(x => x.ClassId, f => f.);

        public static Workshop Generate() => faker.Generate();

        public static List<Workshop> Generate(int count) => faker.Generate(count);

        public static Workshop WithProvider(this Workshop workshop, Provider provider)
            => workshop.ApplyOnItem(x => x.Provider = provider)
                       .ApplyOnItem(x => x.ProviderId = provider.Id);

        public static List<Workshop> WithProvider(this List<Workshop> workshops, Provider provider)
            => workshops.ApplyOnCollection(x => x.WithProvider(provider));

        public static Workshop WithAddress(this Workshop workshop, Address address)
            => workshop.ApplyOnItem(x => x.Address = address)
                       .ApplyOnItem(x => x.AddressId = address.Id);

        public static List<Workshop> WithAddress(this List<Workshop> workshops, Address address)
            => workshops.ApplyOnCollection(x => x.WithAddress(address));

        public static Workshop WithDirection(this Workshop workshop, Direction direction)
            => workshop.ApplyOnItem(x => x.Direction = direction)
                       .ApplyOnItem(x => x.DirectionId = direction.Id);

        public static List<Workshop> WithDirection(this List<Workshop> workshops, Direction direction)
            => workshops.ApplyOnCollection(x => x.WithDirection(direction));

        public static Workshop WithDepartment(this Workshop workshop, Department department)
            => workshop.ApplyOnItem(x => x.DepartmentId = department.Id);

        public static List<Workshop> WithDepartment(this List<Workshop> workshops, Department department)
            => workshops.ApplyOnCollection(x => x.WithDepartment(department));
    }
}
