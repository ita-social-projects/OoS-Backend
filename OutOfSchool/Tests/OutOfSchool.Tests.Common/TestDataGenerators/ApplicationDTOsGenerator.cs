using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    /// <summary>
    /// Contains methods to generate fake <see cref="ApplicationDto"/> objects.
    /// </summary>
    public static class ApplicationDTOsGenerator
    {
        private static readonly TimeSpan timeShift = TimeSpan.FromDays(5);

        private static readonly Faker<ApplicationDto> faker = new Faker<ApplicationDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.CreationTime, f => f.Date.Between(DateTime.Now - timeShift, DateTime.Now + timeShift))
            .RuleFor(x => x.Status, f => f.Random.Enum<ApplicationStatus>());

        /// <summary>
        /// Generates new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <returns><see cref="ApplicationDto"/> object with random data.</returns>
        public static ApplicationDto Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="ApplicationDto"/> objects.
        /// </summary>
        /// <param name="number">Number of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ApplicationDto"/> objects.</returns>
        public static List<ApplicationDto> Generate(int number) => faker.Generate(number);

        /// <summary>
        /// Assigns given <paramref name="workshop"/> to the given <paramref name="application"/>
        /// </summary>
        /// <returns><see cref="ApplicationDto"/> object with assigned <paramref name="workshopId"/>.</returns>
        public static ApplicationDto WithWorkshopDto(this ApplicationDto application, WorkshopDTO workshop)
        {
            _ = application ?? throw new ArgumentNullException(nameof(application));
            _ = workshop ?? throw new ArgumentNullException(nameof(workshop));

            application.Workshop = workshop;
            application.WorkshopId = workshop.Id;

            return application;
        }

        /// <summary>
        /// Assigns given <paramref name="workshop"/> to the given <paramref name="applications"/>
        /// </summary>
        /// <returns><see cref="ApplicationDto"/> object with assigned <paramref name="workshopId"/>.</returns>
        public static List<ApplicationDto> WithWorkshopDto(this List<ApplicationDto> applications, WorkshopDTO workshop)
        {
            _ = applications ?? throw new ArgumentNullException(nameof(applications));
            _ = workshop ?? throw new ArgumentNullException(nameof(workshop));

            applications.ForEach(a => a.WithWorkshopDto(workshop));

            return applications;
        }
    }
}
