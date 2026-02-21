using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class InstitutionStatusDTO
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public static class InstitutionStatusDTOExtensions
{
    public static InstitutionStatus ToModel(this InstitutionStatusDTO dto)
        => new()
        {
            Id = dto.Id,
            Name = dto.Name,
        };

    public static InstitutionStatusDTO ToDto(this InstitutionStatus model)
        => new()
        {
            Id = model.Id,
            Name = model.Name,
        };

    public static List<InstitutionStatusDTO> ToDto(this IEnumerable<InstitutionStatus> list)
        => list.MapToList(ToDto);

}