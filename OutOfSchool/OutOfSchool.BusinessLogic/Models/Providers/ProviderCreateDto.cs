﻿using OutOfSchool.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderCreateDto : ProviderBaseDto
{
    [Required]
    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType Ownership { get; set; }
}
