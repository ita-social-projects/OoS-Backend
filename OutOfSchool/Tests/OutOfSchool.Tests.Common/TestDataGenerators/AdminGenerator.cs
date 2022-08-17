using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class AdminGenerator
{
    private static readonly Faker<MinistryAdminDto> Faker = new Faker<MinistryAdminDto>()
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x=>x.InstitutionId, _ => Guid.NewGuid());

    private static readonly Faker<InstitutionAdmin> FakerInstitutionAdmin = new Faker<InstitutionAdmin>()
        .RuleFor(x => x.Id,f => f.Random.Long() )
        .RuleFor(x => x.InstitutionId, Guid.NewGuid())
        .RuleFor(x => x.Id, f =>f.Random.Int());
    public static MinistryAdminDto GenerateMinistryAdminDto() => Faker.Generate();
    public static InstitutionAdmin GenerateInstitutionAdmin() => FakerInstitutionAdmin.Generate();
    public static List<MinistryAdminDto> GenerateMinistryAdminsDtos(int count) => Faker.Generate(count);
    public static List<InstitutionAdmin> GenerateInstitutionAdmins(int count) => FakerInstitutionAdmin.Generate(count);
    public static InstitutionAdmin WithUserAndInstitution(this InstitutionAdmin institutionAdmin)
    {
        institutionAdmin.User = UserGenerator.Generate();
        institutionAdmin.Institution = InstitutionsGenerator.Generate();
        return institutionAdmin;
    }
    public static List<InstitutionAdmin> WithUserAndInstitution(this List<InstitutionAdmin> institutionAdmin)
    {
        institutionAdmin.ForEach(x=>x.User = UserGenerator.Generate());
        institutionAdmin.ForEach(x=>x.Institution = InstitutionsGenerator.Generate());
        return institutionAdmin;
    }
}