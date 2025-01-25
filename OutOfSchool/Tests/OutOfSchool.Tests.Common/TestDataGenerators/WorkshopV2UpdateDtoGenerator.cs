using System.Collections.Generic;
using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops.V2;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public class WorkshopV2UpdateDtoGenerator
{
    private static readonly Faker<WorkshopUpdateV2Dto> Faker = new Faker<WorkshopUpdateV2Dto>()
        .RuleFor(x => x.CoverImageId, f => f.Image.LoremFlickrUrl())
        .RuleFor(x => x.ImageIds, _ => new List<string>())
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopUpdateV2Dto();
            WorkshopUpdateDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopUpdateV2Dto Generate() => Faker.Generate();

    public static List<WorkshopUpdateV2Dto> Generate(int count) => Faker.Generate(count);
}