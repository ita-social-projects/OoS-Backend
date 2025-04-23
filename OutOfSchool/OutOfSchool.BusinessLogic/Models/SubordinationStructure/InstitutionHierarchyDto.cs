using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.SubordinationStructure;

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

    public List<SubDirectionDto> SubDirections { get; set; }
}

public static class InstitutionHierarchyDtoExtensions
{
    public static InstitutionHierarchy SetToModel(this InstitutionHierarchyDto dto, InstitutionHierarchy model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.HierarchyLevel = dto.HierarchyLevel;
        model.ParentId = dto.ParentId;
        model.InstitutionId = dto.InstitutionId;

        return model;
    }

    public static InstitutionHierarchy ToModel(this InstitutionHierarchyDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            HierarchyLevel = dto.HierarchyLevel,
            ParentId = dto.ParentId,
            InstitutionId = dto.InstitutionId,
        };

    public static List<InstitutionHierarchy> ToModel(this IEnumerable<InstitutionHierarchyDto> list)
        => list.MapToList(ToModel);

    public static InstitutionHierarchyDto ToDto(this InstitutionHierarchy model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            HierarchyLevel = model.HierarchyLevel,
            ParentId = model.ParentId,
            InstitutionId = model.InstitutionId,
            Institution = model.Institution.ToDto(),
            SubDirections = model.SubDirections.ToDto()
        };

    public static List<InstitutionHierarchyDto> ToDto(this IEnumerable<InstitutionHierarchy> list)
        => list.MapToList(ToDto);
}