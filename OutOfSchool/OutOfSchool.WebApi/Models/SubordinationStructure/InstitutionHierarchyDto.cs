using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.SubordinationStructure;

public class InstitutionHierarchyDto
{
    public Guid Id { get; set; }

    [MinLength(1)]
    [MaxLength(200)]
    public string Title { get; set; }

    public int HierarchyLevel { get; set; }

    public Guid? ParentId { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }

    public InstitutionDto Institution { get; set; }

    public List<DirectionDto> Directions { get; set; }
}