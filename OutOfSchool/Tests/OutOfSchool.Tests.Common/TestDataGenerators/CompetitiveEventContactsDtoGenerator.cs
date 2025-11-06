using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class CompetitiveEventContactsDtoGenerator
{
    private static readonly Faker<CompetitiveEventContactsDto> Faker = new Faker<CompetitiveEventContactsDto>()
        .RuleFor(x => x.Title, f => f.Lorem.Sentence(5))
        .RuleFor(x => x.ShortTitle, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.RegistrationStartTime, f => f.Date.PastOffset(1))
        .RuleFor(x => x.RegistrationEndTime, f => f.Date.FutureOffset(1))
        .RuleFor(x => x.CoverageId, f => f.Random.Int(1, 10))
        .RuleFor(x => x.CompetitiveEventDescriptionItems, f => new List<CompetitiveEventDescriptionItemDto>
        {
            new() {
                Id = f.Random.Guid(),
                Description = f.Lorem.Paragraph(1),
                CompetitiveEventId = f.Random.Guid(),
                SectionName = f.Lorem.Word(),
            }
        })
        .RuleFor(x => x.ScheduledStartTime, f => f.Date.FutureOffset(1))
        .RuleFor(x => x.ScheduledEndTime, f => f.Date.FutureOffset(2))
        .RuleFor(x => x.NumberOfSeats, f => f.Random.UInt(1, 100))
        .RuleFor(x => x.CompetitiveEventAccountingTypeId, f => f.Random.Int(1, 10))
        .RuleFor(x => x.MinimumAge, f => f.Random.Int(5, 8))
        .RuleFor(x => x.MaximumAge, f => f.Random.Int(9, 15))
        .RuleFor(x => x.Base64CoverImage, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.Base64ImageFiles, f => new List<string>() { f.Image.LoremFlickrUrl() })
        .RuleFor(x => x.Contacts, f => new List<ContactsDto> { })
        .RuleFor(x => x.SubDirectionIds, f => new List<long> { f.Random.Long(1, 100) })
        .RuleFor(x => x.AreThereBenefits, true)
        .RuleFor(x => x.Benefits, f => f.Lorem.Sentences(3))
        .RuleFor(x => x.CompetitiveSelection, true)
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentences(3))
        .RuleFor(x => x.VenueName, f => f.Lorem.Sentences(3))
        .RuleFor(x => x.DescriptionOfTheEnrollmentProcedure, f => f.Lorem.Sentences(3))
        .RuleFor(x => x.IsPaid, true)
        .RuleFor(x => x.Price, f => f.Random.Int(1, 100000))
        .RuleFor(x => x.PlannedFormatOfClasses, f => f.PickRandom<FormOfLearning>());

    public static CompetitiveEventContactsDto Generate() => Faker.Generate();

    public static List<CompetitiveEventContactsDto> Generate(int count) => Faker.Generate(count);
}
