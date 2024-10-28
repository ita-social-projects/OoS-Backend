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