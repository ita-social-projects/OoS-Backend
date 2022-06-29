﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;

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
        .RuleFor(x => x.WorkshopDescriptionItems, f => f.Make(new Random().Next(1, 4), () =>
            new WorkshopDescriptionItem()
            {
                Id = Guid.NewGuid(),
                SectionName = f.Lorem.Sentence(),
                Description = f.Lorem.Paragraph(),
            }))
        .RuleFor(x => x.WithDisabilityOptions, f => f.Random.Bool())
        .RuleFor(x => x.DisabilityOptionsDesc, f => f.Lorem.Sentence())
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ProviderTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.Keywords, f => f.Lorem.Sentence())
        .RuleFor(x => x.PayRate, f => f.PickRandom<PayRateType>());

    public static Workshop Generate() => faker.Generate();

    public static List<Workshop> Generate(int count) => faker.Generate(count);

    public static Workshop WithProvider(this Workshop workshop, Provider provider)
        => TestDataHelper.ApplyOnItem(workshop, (workshop, provider) => { workshop.Provider = provider; workshop.ProviderId = provider.Id; }, provider);

    public static List<Workshop> WithProvider(this List<Workshop> workshops, Provider provider)
        => TestDataHelper.ApplyOnCollection(workshops, (workshop, provider) => WithProvider(workshop, provider), provider);

    public static Workshop WithAddress(this Workshop workshop, Address address = null)
    {
        address ??= AddressGenerator.Generate();
        return TestDataHelper.ApplyOnItem(workshop, (workshop, address) => { workshop.Address = address; workshop.AddressId = address.Id; }, address);
    }

    public static List<Workshop> WithAddress(this List<Workshop> workshops, Address address = null)
        => workshops.Select(x => WithAddress(x, address)).ToList();

    public static Workshop WithDirection(this Workshop workshop, Direction direction)
        => TestDataHelper.ApplyOnItem(workshop, (workshop, direction) => { workshop.Direction = direction; workshop.DirectionId = direction.Id; }, direction);

    public static List<Workshop> WithDirection(this List<Workshop> workshops, Direction direction)
        => TestDataHelper.ApplyOnCollection(workshops, (workshop, direction) => WithDirection(workshop, direction), direction);

    public static Workshop WithDepartment(this Workshop workshop, Department department)
        => TestDataHelper.ApplyOnItem(workshop, (item, value) => { item.DepartmentId = value.Id; }, department);

    public static List<Workshop> WithDepartment(this List<Workshop> workshops, Department department)
        => TestDataHelper.ApplyOnCollection(workshops, (item, value) => WithDepartment(item, value), department);

    public static Workshop WithApplications(this Workshop workshop)
    {
        workshop.Applications = ApplicationGenerator.Generate(new Random().Next(1, 4))
            .WithWorkshop(workshop);
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
}
}
