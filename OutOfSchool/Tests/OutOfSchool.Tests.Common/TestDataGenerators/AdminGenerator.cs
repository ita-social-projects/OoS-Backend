using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class AdminGenerator
{
    private static readonly string PhoneFormat = "##########";

    private static readonly Faker<MinistryAdminDto> Faker = new Faker<MinistryAdminDto>()
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x=>x.InstitutionId, _ => Guid.NewGuid());

    private static readonly Faker<InstitutionAdmin> FakerInstitutionAdmin = new Faker<InstitutionAdmin>()
        .RuleFor(x => x.InstitutionId, Guid.NewGuid())
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString());

    private static readonly Faker<RegionAdmin> FakerRegionAdmin = new Faker<RegionAdmin>()
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long());

    private static readonly Faker<RegionAdminDto> FakerRegionAdminDto = new Faker<RegionAdminDto>()
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long());

    private static readonly Faker<AreaAdmin> FakerAreaAdmin = new Faker<AreaAdmin>()
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long());

    private static readonly Faker<AreaAdminDto> FakerAreaAdminDto = new Faker<AreaAdminDto>()
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long());

    private static readonly Faker<AreaAdminBaseDto> FakerAreaAdminBaseDto = new Faker<AreaAdminBaseDto>()
        .RuleFor(x => x.UserId, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber(PhoneFormat))
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.CATOTTGId, f => f.Random.Long());

    private static readonly Faker<CreateEmployeeDto> FakerCreateProviderAdminDto
        = new Faker<CreateEmployeeDto>()
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber(PhoneFormat));

    public static MinistryAdminDto GenerateMinistryAdminDto() => Faker.Generate();
    public static InstitutionAdmin GenerateInstitutionAdmin() => FakerInstitutionAdmin.Generate();
    public static RegionAdmin GenerateRegionAdmin() => FakerRegionAdmin.Generate();
    public static RegionAdminDto GenerateRegionAdminDto() => FakerRegionAdminDto.Generate();
    public static AreaAdmin GenerateAreaAdmin() => FakerAreaAdmin.Generate();
    public static AreaAdminDto GenerateAreaAdminDto() => FakerAreaAdminDto.Generate();
    public static AreaAdminBaseDto GenerateAreaAdminBaseDto() => FakerAreaAdminBaseDto.Generate();
    public static CreateEmployeeDto GenerateCreateEmployeeDto() => FakerCreateProviderAdminDto.Generate();
    public static List<MinistryAdminDto> GenerateMinistryAdminsDtos(int count) => Faker.Generate(count);
    public static List<InstitutionAdmin> GenerateInstitutionAdmins(int count) => FakerInstitutionAdmin.Generate(count);
    public static List<RegionAdmin> GenerateRegionAdmins(int count) => FakerRegionAdmin.Generate(count);
    public static List<RegionAdminDto> GenerateRegionAdminsDtos(int count) => FakerRegionAdminDto.Generate(count);
    public static List<AreaAdmin> GenerateAreaAdmins(int count) => FakerAreaAdmin.Generate(count);
    public static List<AreaAdminDto> GenerateAreaAdminsDtos(int count) => FakerAreaAdminDto.Generate(count);
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

    public static RegionAdmin WithUserAndInstitution(this RegionAdmin regionAdmin)
    {
        regionAdmin.User = UserGenerator.Generate();
        regionAdmin.Institution = InstitutionsGenerator.Generate();
        return regionAdmin;
    }

    public static List<RegionAdmin> WithUserAndInstitution(this List<RegionAdmin> regionAdmin)
    {
        regionAdmin.ForEach(x => x.User = UserGenerator.Generate());
        regionAdmin.ForEach(x => x.Institution = InstitutionsGenerator.Generate());
        return regionAdmin;
    }
    public static AreaAdmin WithUserAndInstitution(this AreaAdmin areaAdmin)
    {
        areaAdmin.User = UserGenerator.Generate();
        areaAdmin.Institution = InstitutionsGenerator.Generate();
        return areaAdmin;
    }

    public static List<AreaAdmin> WithUserAndInstitution(this List<AreaAdmin> areaAdmin)
    {
        areaAdmin.ForEach(x => x.User = UserGenerator.Generate());
        areaAdmin.ForEach(x => x.Institution = InstitutionsGenerator.Generate());
        return areaAdmin;
    }
}