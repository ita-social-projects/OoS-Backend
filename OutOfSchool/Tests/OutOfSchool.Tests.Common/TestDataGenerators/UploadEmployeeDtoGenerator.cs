using Bogus;
using OutOfSchool.BusinessLogic.Models.Individual;
using System.Collections.Generic;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="UploadEmployeeRequestDto"/> objects.
/// </summary>
public static class UploadEmployeeDtoGenerator
{
    private static readonly string RnokppFormat = "##########";

    private static readonly Faker<UploadEmployeeRequestDto> faker = new Faker<UploadEmployeeRequestDto>()
        .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Rnokpp, f => f.Phone.PhoneNumber(RnokppFormat))
        .RuleFor(x => x.AssignedRole, f => f.Music.Genre());


    /// <summary>
    /// Creates new instance of the <see cref="UploadEmployeeRequestDto"/> class with random data.
    /// </summary>
    /// <returns><see cref="UploadEmployeeRequestDto"/> object.</returns>
    public static UploadEmployeeRequestDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="UploadEmployeeRequestDto"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<UploadEmployeeRequestDto> Generate(int count) => faker.Generate(count);

    /// <summary>
    /// Populates an existing instance of the <see cref="UploadEmployeeRequestDto"/> class with random data.
    /// </summary>
    public static void Populate(UploadEmployeeRequestDto dto) => faker.Populate(dto);
}
