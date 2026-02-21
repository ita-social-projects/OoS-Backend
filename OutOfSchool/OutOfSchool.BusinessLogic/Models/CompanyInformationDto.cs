using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class CompanyInformationDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; }

    [JsonIgnore]
    [Required]
    public CompanyInformationType Type { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<CompanyInformationItemDto> CompanyInformationItems { get; set; }
}

public static class CompanyInformationDtoExtensions
{
    public static CompanyInformation ToModel(this CompanyInformationDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Type = dto.Type,
            CompanyInformationItems = dto.CompanyInformationItems?.ToModel() ?? []
        };

    public static List<CompanyInformation> ToModel(this IEnumerable<CompanyInformationDto> list)
        => list.MapToList(ToModel);

    public static CompanyInformationDto ToDto(this CompanyInformation model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Type = model.Type,
            CompanyInformationItems = model.CompanyInformationItems?.ToDto() ?? []
        };

    public static List<CompanyInformationDto> ToDto(this IEnumerable<CompanyInformation> list)
        => list.MapToList(ToDto);
}