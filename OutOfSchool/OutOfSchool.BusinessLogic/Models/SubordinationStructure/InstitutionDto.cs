using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.SubordinationStructure;

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

public static class InstitutionDtoExtensions
{
    public static Institution SetToModel(this InstitutionDto dto, Institution model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.NumberOfHierarchyLevels = dto.NumberOfHierarchyLevels;
        model.IsGovernment = dto.IsGovernment;

        return model;
    }

    public static Institution ToModel(this InstitutionDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            NumberOfHierarchyLevels = dto.NumberOfHierarchyLevels,
            IsGovernment = dto.IsGovernment,
        };

    public static List<Institution> ToModel(this IEnumerable<InstitutionDto> list)
        => list.MapToList(ToModel);

    public static InstitutionDto ToDto(this Institution model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            NumberOfHierarchyLevels = model.NumberOfHierarchyLevels,
            IsGovernment = model.IsGovernment,
        };

    public static List<InstitutionDto> ToDto(this IEnumerable<Institution> list)
        => list.MapToList(ToDto);
}