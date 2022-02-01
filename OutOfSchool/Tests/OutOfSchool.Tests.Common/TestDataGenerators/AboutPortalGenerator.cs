using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class AboutPortalGenerator
    {
        private static Faker<AboutPortal> faker = new Faker<AboutPortal>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.Title, f => f.Company.CompanyName());

        public static AboutPortal Geterate() => faker.Generate();
    }
}
