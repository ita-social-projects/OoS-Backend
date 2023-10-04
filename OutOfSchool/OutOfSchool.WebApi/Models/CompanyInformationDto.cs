﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Util.JsonTools;

namespace OutOfSchool.WebApi.Models;

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