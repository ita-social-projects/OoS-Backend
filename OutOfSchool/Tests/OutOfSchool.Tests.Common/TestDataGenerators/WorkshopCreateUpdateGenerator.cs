using Bogus;
using OutOfSchool.BusinessLogic.Models.Workshops;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopCreateUpdateDtoGenerator
{
    private static readonly Faker<WorkshopCreateUpdateDto> Faker = new Faker<WorkshopCreateUpdateDto>()

    .RuleFor(x => x.TagIds, f => f.Make(5, () => f.Random.Long(1, 100)))
    .CustomInstantiator(f =>
    {
        var dto = new WorkshopCreateUpdateDto();
        WorkshopBaseDtoGenerator.Populate(dto);
        return dto;
    });
    
    public static WorkshopCreateUpdateDto Generate() => Faker.Generate();

    public static List<WorkshopCreateUpdateDto> Generate(int count) => Faker.Generate(count);

    public static void Populate(WorkshopCreateUpdateDto dto) => Faker.Populate(dto);
}