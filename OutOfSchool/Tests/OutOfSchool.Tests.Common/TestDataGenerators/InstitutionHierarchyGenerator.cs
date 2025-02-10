using System;
using System.Collections.Generic;
using Bogus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class InstitutionHierarchyGenerator
{
    private static readonly Faker<InstitutionHierarchy> faker = new Faker<InstitutionHierarchy>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Title, f => f.Company.CompanyName())
        .RuleFor(x => x.InstitutionId, _ => Guid.NewGuid())
        .RuleFor(x => x.Directions, _ => []);

    public static InstitutionHierarchy Generate() => faker.Generate();
    
    public static List<InstitutionHierarchy> Generate(int count) => faker.Generate(count);

    public static InstitutionHierarchy WithId(this InstitutionHierarchy hierarchy, Guid id)
    {
        hierarchy.Id = id;
        return hierarchy;
    }
    
    public static InstitutionHierarchy WithParentId(this InstitutionHierarchy hierarchy, Guid parentId)
    {
        hierarchy.ParentId = parentId;
        return hierarchy;
    }
    
    public static InstitutionHierarchy WithInstitutionId(this InstitutionHierarchy hierarchy, Guid institutionId)
    {
        hierarchy.InstitutionId = institutionId;
        return hierarchy;
    }
    
    public static InstitutionHierarchy WithDirections(this InstitutionHierarchy hierarchy, List<Direction> directions)
    {
        hierarchy.Directions = directions;
        return hierarchy;
    }
    
    public static InstitutionHierarchy WithLevel(this InstitutionHierarchy institution, int level)
    {
        institution.HierarchyLevel = level;
        return institution;
    }
}
