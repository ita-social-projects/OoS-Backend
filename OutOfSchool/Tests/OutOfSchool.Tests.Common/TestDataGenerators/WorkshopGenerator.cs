using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

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
        .RuleFor(x => x.Price, f => f.Random.Decimal())
        .RuleFor(x => x.WorkshopDescriptionItems, f => WorkshopDescriptionItemGenerator.Generate(4))
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.DisabilityOptionsDesc, f => f.Lorem.Sentence())
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.Keywords, f => f.Lorem.Sentence())
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>())
        .RuleFor(x => x.UpdatedAt, _ => DateTime.Now)
        .RuleFor(x => x.FormOfLearning, f => f.PickRandom<FormOfLearning>())
        .RuleFor(x => x.ActiveFrom, (f, w) => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DateOnly.FromDateTime(DateTime.Now)))
        .RuleFor(x => x.ActiveTo, f => f.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(300))))
        .RuleFor(x => x.EnrollmentProcedureDescription, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.AreThereBenefits, _ => true)
        .RuleFor(x => x.PreferentialTermsOfParticipation, f => f.Lorem.Sentences(3))
        .RuleFor(x => x.CompetitiveSelection, _ => true)
        .RuleFor(x => x.CompetitiveSelectionDescription, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.IsPaid, _ => true)
        .RuleFor(x => x.IsSelfFinanced, f => f.Random.Bool())
        .RuleFor(x => x.IsSpecial, _ => false)
        .RuleFor(x => x.ShortStay, f => f.Random.Bool())
        .RuleFor(x => x.ShortTitle, f => f.Company.CompanyName());

    public static Workshop Generate() => faker.Generate();

    public static List<Workshop> Generate(int count) => faker.Generate(count);

    public static Workshop WithId(this Workshop workshop, Guid id)
    {
        workshop.Id = id;
        return workshop;
    }

    public static Workshop WithProvider(this Workshop workshop, Provider provider = null)
    {
        provider ??= ProvidersGenerator.Generate();
        return TestDataHelper.ApplyOnItem(workshop, (workshop, provider) => { workshop.Provider = provider; workshop.ProviderId = provider.Id; }, provider);
    }

    public static List<Workshop> WithProvider(this List<Workshop> workshops, Provider provider = null)
        => TestDataHelper.ApplyOnCollection(workshops, (workshop, provider) => WithProvider(workshop, provider), provider);

    public static Workshop WithAddress(this Workshop workshop, Address address = null)
    {
        address ??= AddressGenerator.Generate();
        return TestDataHelper.ApplyOnItem(workshop, (workshop, address) => { workshop.Address = address; workshop.AddressId = address.Id; }, address);
    }

    public static List<Workshop> WithAddress(this List<Workshop> workshops, Address address = null)
        => workshops.Select(x => WithAddress(x, address)).ToList();

    public static Workshop WithInstitutionHierarchy(this Workshop workshop, InstitutionHierarchy direction)
        => TestDataHelper.ApplyOnItem(workshop, (workshop, direction) => { workshop.InstitutionHierarchy = direction; workshop.InstitutionHierarchyId = direction.Id; }, direction);

    public static List<Workshop> WithInstitutionHierarchy(this List<Workshop> workshops, InstitutionHierarchy direction)
        => TestDataHelper.ApplyOnCollection(workshops, (workshop, direction) => WithInstitutionHierarchy(workshop, direction), direction);

    public static Workshop WithApplications(this Workshop workshop)
    {
        workshop.Applications = ApplicationGenerator.Generate(new Random().Next(1, 4))
            .WithWorkshop(workshop)
            .WithParent(ParentGenerator.Generate())
            .WithChild(ChildGenerator.Generate());
        return workshop;
    }

    public static List<Workshop> WithApplications(this List<Workshop> workshops)
        => workshops.Select(x => x.WithApplications()).ToList();

    public static Workshop WithTeachers(this Workshop workshop)
    {
        workshop.Teachers = TeachersGenerator.Generate(new Random().Next(1, 4))
            .WithWorkshop(workshop);
        return workshop;
    }

    public static List<Workshop> WithTeachers(this List<Workshop> workshops)
        => workshops.Select(x => x.WithTeachers()).ToList();

    public static Workshop WithImages(this Workshop workshop)
    {
        workshop.Images = ImagesGenerator.Generate<Workshop>(new Random().Next(1, 4), workshop);
        return workshop;
    }

    public static List<Workshop> WithImages(this List<Workshop> workshops)
        => workshops.Select(x => x.WithImages()).ToList();

    public static Workshop WithTags(this Workshop workshop)
    {
        workshop.Tags = TagsGenerator.Generate(new Random().Next(1, 4))
            .WithWorkshop(workshop);
        return workshop;
    }
}
