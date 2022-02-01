using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class AboutPortalItemGenerator
    {
        private static Faker<AboutPortalItem> faker = new Faker<AboutPortalItem>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.SectionName, f => f.Company.CompanyName())
            .RuleFor(x => x.Description, f => f.Random.Words(10));

        public static AboutPortalItem Generate() => faker.Generate();

        public static List<AboutPortalItem> Generate(int count) => faker.Generate(count);

        public static AboutPortalItem WithAboutPortal(this AboutPortalItem aboutPortalItem, AboutPortal aboutPortal) =>
            TestDataHelper.ApplyOnItem(aboutPortalItem, (aboutPortalItem, aboutPortal) =>
            { aboutPortalItem.AboutPortal = aboutPortal; aboutPortalItem.AboutPortalId = aboutPortal.Id; }, aboutPortal);
    }
}
