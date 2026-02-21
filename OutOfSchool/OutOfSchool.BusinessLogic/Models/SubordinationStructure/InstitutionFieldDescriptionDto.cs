using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.SubordinationStructure;

public class InstitutionFieldDescriptionDto
{
    public Guid Id { get; set; }

    [MinLength(1)]
    [MaxLength(100)]
    public string Title { get; set; }

    public int HierarchyLevel { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }
}

public static class InstitutionFieldDescriptionDtoExtensions
{
    public static InstitutionFieldDescriptionDto ToDto(this InstitutionFieldDescription model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            HierarchyLevel = model.HierarchyLevel,
            InstitutionId = model.InstitutionId
        };

    public static List<InstitutionFieldDescriptionDto> ToDto(this IEnumerable<InstitutionFieldDescription> list)
        => list.MapToList(ToDto);
}