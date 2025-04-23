using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class CompanyInformationItemDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid CompanyInformationId { get; set; }
}

public static class CompanyInformationItemDtoExtensions
{
    public static CompanyInformationItem ToModel(this CompanyInformationItemDto dto)
        => new()
        {
            Id = dto.Id,
            SectionName = dto.SectionName,
            Description = dto.Description,
            CompanyInformationId = dto.CompanyInformationId
        };

    public static List<CompanyInformationItem> ToModel(this IEnumerable<CompanyInformationItemDto> list)
        => list.MapToList(ToModel);

    public static CompanyInformationItemDto ToDto(this CompanyInformationItem model)
        => new()
        {
            Id = model.Id,
            SectionName = model.SectionName,
            Description = model.Description,
            CompanyInformationId = model.CompanyInformationId
        };

    public static List<CompanyInformationItemDto> ToDto(this IEnumerable<CompanyInformationItem> list)
        => list.MapToList(ToDto);
}