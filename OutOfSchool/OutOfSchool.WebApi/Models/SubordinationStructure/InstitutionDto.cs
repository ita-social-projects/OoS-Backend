using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.SubordinationStructure;

public class InstitutionDto
{
    public Guid Id { get; set; }

    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfHierarchyLevels { get; set; }

    public bool IsGovernment { get; set; }
}