using System;
using System.Collections.Generic;
using AutoMapper.Internal;
using Bogus;
using Nest;
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
        .RuleFor(x => x.InstitutionId, Guid.NewGuid())
        .RuleFor(x => x.UserId, Guid.NewGuid().ToString());
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

    public static InstitutionAdmin WithInstitutionId(this InstitutionAdmin institutionAdmin, Guid institutionId)
    {
        institutionAdmin.InstitutionId = institutionId;
        return institutionAdmin;
    }

    public static List<InstitutionAdmin> WithInsitutionId(this List<InstitutionAdmin> institutionAdmin, Guid institutionId, params int[] itemIndexes)
    {
        if (itemIndexes.Length == 0)
        {
            institutionAdmin.ForEach(x => x.WithInstitutionId(institutionId));
        }
        else
        {
            foreach (var index in itemIndexes)
            {
                if (index < institutionAdmin.Count)
                {
                    institutionAdmin[index].WithInstitutionId(institutionId);
                }
            }
        }
        
        return institutionAdmin;
    }
}