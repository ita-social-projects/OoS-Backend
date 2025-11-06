using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class CompetitiveEventDraftContentGenerator
{
    private static readonly Faker<CompetitiveEventDraftContent> Faker = new Faker<CompetitiveEventDraftContent>()
        .RuleFor(x => x.Title, f => f.Lorem.Sentence(5))
        .RuleFor(x => x.ShortTitle, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.RegistrationStartTime, f => f.Date.PastOffset(1))
        .RuleFor(x => x.RegistrationEndTime, f => f.Date.FutureOffset(1))
        .RuleFor(x => x.ParentId, f => f.Random.Guid())
        .RuleFor(x => x.CompetitiveEventDescriptionItems, f => new List<CompetitiveEventDescriptionItem>
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
        .RuleFor(x => x.OrganizerOfTheEventId, f => f.Random.Guid())
        .RuleFor(x => x.MinimumAge, f => f.Random.Int(5, 18))
        .RuleFor(x => x.Contacts, f => new List<Contacts> { })
        .RuleFor(x => x.SubDirectionIds, f => new List<long> { f.Random.Long(1, 100) })
        .RuleFor(x => x.AreThereBenefits, true)
        .RuleFor(x => x.Benefits, f => f.Lorem.Sentence(10))
        .RuleFor(x => x.CompetitiveSelection, true)
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence(10))
        .RuleFor(x => x.IsPaid, true)
        .RuleFor(x => x.Price, f => f.Random.UInt(1, 100000));

    public static CompetitiveEventDraftContent Generate() => Faker.Generate();

    public static List<CompetitiveEventDraftContent> Generate(int count) => Faker.Generate(count);
}