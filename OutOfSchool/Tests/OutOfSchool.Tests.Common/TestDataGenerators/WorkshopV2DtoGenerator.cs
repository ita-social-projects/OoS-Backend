using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopV2DtoGenerator
{
    private static readonly Faker<WorkshopV2Dto> Faker = new Faker<WorkshopV2Dto>()
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ImageIds, _ => new List<string>())
        .RuleFor(x => x.StudyPeriodDates, f => new StudyPeriodDatesDto
        {
            StartDate = new DateOnly(2025, 9, 1),
            EndDate = new DateOnly(2026, 5, 31),
        })
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopV2Dto();
            WorkshopDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopV2Dto Generate() => Faker.Generate();

    public static List<WorkshopV2Dto> Generate(int count) => Faker.Generate(count);
}